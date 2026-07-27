namespace BusTracking.Web.Areas.BusCoordinator.Controllers
{
    [Area("BusCoordinator"), Authorize(Roles = "BusCoordinator")]
    public class BroadcastController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly IFcmPushNotificationService _fcmPushService;

        public BroadcastController(
            AppDbContext db,
            ICurrentUserService currentUser,
            IFcmPushNotificationService fcmPushService)
        {
            _db = db;
            _currentUser = currentUser;
            _fcmPushService = fcmPushService;
        }

        private bool HasPermission() =>
            PermissionHelper.Can(User, "broadcast.manage", HttpContext);

        public async Task<IActionResult> Index()
        {
            if (!HasPermission()) return Forbid();

            var model = new BroadcastModel
            {
                Roles = await GetRolesSelectListAsync()
            };
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetUsersByRole(int roleId)
        {
            if (!HasPermission()) return Forbid();

            var schoolId = _currentUser.SchoolId;

            var query = _db.Users
                .IgnoreQueryFilters()
                .Where(u => u.RoleId == roleId && u.IsActive);

            if (schoolId.HasValue)
            {
                query = query.Where(u => u.SchoolId == schoolId.Value || u.SchoolId == null);
            }

            var users = await query
                .OrderBy(u => u.FullName)
                .Select(u => new
                {
                    userId = u.UserId,
                    fullName = u.FullName,
                    userName = u.UserName,
                    email = u.Email ?? "",
                    phoneNumber = u.PhoneNumber ?? ""
                })
                .ToListAsync();

            return Json(users);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(BroadcastModel model)
        {
            if (!HasPermission()) return Forbid();

            model.Roles = await GetRolesSelectListAsync();

            if (model.SelectedUserIds == null || !model.SelectedUserIds.Any())
            {
                ModelState.AddModelError("SelectedUserIds", "Please select at least one recipient user.");
            }

            if (!ModelState.IsValid)
            {
                return View(nameof(Index), model);
            }

            var schoolId = _currentUser.SchoolId;
            var now = DateTime.UtcNow;

            Enum.TryParse<NotificationType>(model.NotificationType, out var notifType);

            var userIds = (model.SelectedUserIds ?? []).Distinct().ToList();
            var notifications = userIds.Select(userId => new Notification
            {
                RecipientUserId = userId,
                SchoolId = schoolId,
                Title = model.Title.Trim(),
                Body = model.Body.Trim(),
                NotificationType = notifType,
                SentAt = now,
                IsRead = false
            }).ToList();

            await _db.Notifications.AddRangeAsync(notifications);
            await _db.SaveChangesAsync();

            // Dispatch FCM Push Notifications to recipients' active device tokens
            _ = Task.Run(() => _fcmPushService.SendBroadcastPushAsync(userIds, model.Title.Trim(), model.Body.Trim(), model.NotificationType));

            TempData["Success"] = $"Broadcast notification successfully sent to {notifications.Count} recipient(s).";
            return RedirectToAction(nameof(Index));
        }

        private async Task<List<SelectListItem>> GetRolesSelectListAsync()
        {
            var roles = await _db.Roles.Where(r => r.IsActive).OrderBy(r => r.RoleId).ToListAsync();
            return roles.Select(r => new SelectListItem
            {
                Value = r.RoleId.ToString(),
                Text = GetFriendlyRoleName(r.RoleName)
            }).ToList();
        }

        private static string GetFriendlyRoleName(string roleName)
        {
            return roleName switch
            {
                "SuperAdmin" => "Super Admin",
                "BusCoordinator" => "Coordinator",
                "Driver" => "Driver",
                "Parent" => "Parent",
                "Student" => "Student",
                _ => roleName
            };
        }
    }
}
