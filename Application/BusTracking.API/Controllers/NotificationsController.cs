namespace BusTracking.API.Controllers
{
    [Authorize, Route("api/[controller]")]
    public class NotificationsController : ApiBaseController
    {
        private readonly INotificationService _notif;
        public NotificationsController(INotificationService notif) => _notif = notif;

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var r = await _notif.GetUserNotificationsAsync(CurrentUserId);
            return Ok(r);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int page = 1,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var r = await _notif.GetUserNotificationsPagedAsync(CurrentUserId, page, fromDate, toDate);
            return Ok(r);
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var r = await _notif.MarkAsReadAsync(id, CurrentUserId);
            return Ok(r);
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            var r = await _notif.MarkAllAsReadAsync(CurrentUserId);
            return Ok(r);
        }

        // Register device token for push notifications (physical devices only)
        [HttpPost("device-token")]
        public async Task<IActionResult> RegisterDevice([FromBody] RegisterDeviceRequest req,
            [FromServices] AppDbContext db)
        {
            if (req.IsVirtual || req.Platform.Equals("Virtual", StringComparison.OrdinalIgnoreCase))
            {
                return Ok(ApiResponse<bool>.Ok(true, "Virtual/Emulator device token skipped."));
            }

            if (string.IsNullOrWhiteSpace(req.Token))
            {
                return BadRequest(ApiResponse<bool>.Fail("Token is required."));
            }

            if (!Enum.TryParse<DevicePlatform>(req.Platform, true, out var platform))
                platform = DevicePlatform.Android;

            // Delete any previous record for the same token across ALL users
            // (Ensures that if User A logs out and User B logs in on the same phone,
            // the token is reassigned to User B and deleted from User A).
            var existingTokens = await db.DeviceTokens
                .IgnoreQueryFilters()
                .Where(d => d.Token == req.Token)
                .ToListAsync();

            if (existingTokens.Count > 0)
            {
                db.DeviceTokens.RemoveRange(existingTokens);
            }

            var user = await db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.UserId == CurrentUserId);
            db.DeviceTokens.Add(new DeviceToken
            {
                SchoolId = user?.SchoolId,
                UserId = CurrentUserId,
                Token = req.Token,
                Platform = platform,
                IsActive = true,
                RegisteredAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
            return Ok(ApiResponse<bool>.Ok(true, "Device registered successfully."));
        }

        // Remove device token on logout
        [HttpPost("device-token/remove")]
        public async Task<IActionResult> RemoveDevice([FromBody] RemoveDeviceRequest req,
            [FromServices] AppDbContext db)
        {
            if (string.IsNullOrWhiteSpace(req.Token))
            {
                return Ok(ApiResponse<bool>.Ok(true, "No token provided."));
            }

            var existingTokens = await db.DeviceTokens
                .IgnoreQueryFilters()
                .Where(d => d.Token == req.Token)
                .ToListAsync();

            if (existingTokens.Count > 0)
            {
                db.DeviceTokens.RemoveRange(existingTokens);
                await db.SaveChangesAsync();
            }

            return Ok(ApiResponse<bool>.Ok(true, "Device token removed cleanly."));
        }

        public class RegisterDeviceRequest
        {
            public string Token { get; set; } = "";
            public string Platform { get; set; } = "";   // Android | iOS
            public bool IsVirtual { get; set; } = false;
        }

        public class RemoveDeviceRequest
        {
            public string Token { get; set; } = "";
        }
    }
}
