namespace BusTracking.API.Controllers
{
    /// <summary>
    /// All endpoints here are for the Driver role only.
    /// Route: /api/driver/...
    /// </summary>
    [Authorize(Roles = "Driver"), Route("api/driver")]
    public class DriverController : ApiBaseController
    {
        private readonly AppDbContext _db;
        private readonly INotificationService _notif;
        private readonly IDriverTripWebService _driverTrip;

        public DriverController(
            AppDbContext db,
            INotificationService notif,
            IDriverTripWebService driverTrip)
        {
            _db = db;
            _notif = notif;
            _driverTrip = driverTrip;
        }

        // ══════════════════════════════════════════════════════════════════
        // DASHBOARD
        // ══════════════════════════════════════════════════════════════════

        /// <summary>
        /// GET /api/driver/dashboard
        /// Returns the driver's assigned bus, route, and today's trip summary.
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var r = await _driverTrip.GetMyTripAsync(CurrentUserId);
            return r.Success ? Ok(r) : NotFound(r);
        }

        // ══════════════════════════════════════════════════════════════════
        // PROFILE (driver views their own detail including license/bus info)
        // ══════════════════════════════════════════════════════════════════

        /// <summary>
        /// GET /api/driver/profile
        /// Returns the logged-in driver's full profile including bus and license info.
        /// </summary>
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var user = await _db.Users
                .Include(u => u.DriverDetail)
                .FirstOrDefaultAsync(u => u.UserId == CurrentUserId);

            if (user is null)
                return NotFound(ApiResponse<DriverProfileModel>.Fail("Driver not found."));

            var assignedBuses = await _db.BusDriverMappings
                .Where(dm => dm.DriverUserId == CurrentUserId)
                .Include(dm => dm.Bus)
                    .ThenInclude(b => b.RouteMappings)
                        .ThenInclude(rm => rm.Route)
                .Select(dm => dm.Bus)
                .ToListAsync();

            var busName = assignedBuses.Any() ? string.Join(", ", assignedBuses.Select(b => b.BusName)) : null;
            var busNumber = assignedBuses.Any() ? string.Join(", ", assignedBuses.Select(b => b.BusNumber)) : null;
            var routeName = assignedBuses.Any() ? string.Join(", ", assignedBuses.SelectMany(b => b.RouteMappings.Select(rm => rm.Route.RouteName))) : null;

            var dto = new DriverProfileModel
            {
                UserId = user.UserId,
                FullName = user.FullName ?? "",
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber,
                ProfileImageUrl = user.ProfileImageUrl,
                LicenseNumber = user.DriverDetail?.LicenseNumber,
                LicenseExpiry = user.DriverDetail?.LicenseExpiry?.ToString("yyyy-MM-dd"),
                BusName = busName,
                BusNumber = busNumber,
                RouteName = routeName,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };

            return Ok(ApiResponse<DriverProfileModel>.Ok(dto));
        }

        // ══════════════════════════════════════════════════════════════════
        // NOTIFICATIONS
        // ══════════════════════════════════════════════════════════════════

        /// <summary>
        /// GET /api/driver/notifications
        /// Returns all notifications for the logged-in driver.
        /// </summary>
        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications()
        {
            var r = await _notif.GetUserNotificationsAsync(CurrentUserId);
            return Ok(r);
        }

        /// <summary>
        /// POST /api/driver/notifications/{id}/read
        /// Marks a single notification as read.
        /// </summary>
        [HttpPost("notifications/{id}/read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var r = await _notif.MarkAsReadAsync(id, CurrentUserId);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        /// <summary>
        /// POST /api/driver/notifications/read-all
        /// Marks all driver notifications as read.
        /// </summary>
        [HttpPost("notifications/read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            var r = await _notif.MarkAllAsReadAsync(CurrentUserId);
            return Ok(r);
        }
    }
}
