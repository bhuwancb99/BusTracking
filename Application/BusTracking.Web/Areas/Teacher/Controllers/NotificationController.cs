namespace BusTracking.Web.Areas.Teacher.Controllers
{
    [Area("Teacher"), Authorize(Roles = "Teacher")]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;

        private int CurrentUserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : 0;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _notificationService.GetUserNotificationsAsync(CurrentUserId);
            return View(result.Data ?? new List<BusTracking.Common.DTOs.Notification.NotificationDto>());
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            var result = await _notificationService.MarkAsReadAsync(notificationId, CurrentUserId);
            return Json(new { success = result.Success });
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var result = await _notificationService.MarkAllAsReadAsync(CurrentUserId);
            return Json(new { success = result.Success });
        }
    }
}
