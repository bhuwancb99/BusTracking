using BusTracking.Common.Data;
using BusTracking.Common.DTOs.Common;
using BusTracking.Common.Entities;
using BusTracking.Common.Enums;
using BusTracking.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        // Register device token for push notifications
        [HttpPost("device-token")]
        public async Task<IActionResult> RegisterDevice([FromBody] RegisterDeviceRequest req,
            [FromServices] AppDbContext db)
        {
            if (!Enum.TryParse<DevicePlatform>(req.Platform, true, out var platform))
                return BadRequest(ApiResponse<bool>.Fail("Invalid platform."));

            // Deactivate old tokens for same user
            await db.DeviceTokens
                .Where(d => d.UserId == CurrentUserId && d.Token == req.Token)
                .ExecuteUpdateAsync(s => s.SetProperty(d => d.IsActive, false));

            db.DeviceTokens.Add(new DeviceToken
            {
                UserId = CurrentUserId,
                Token = req.Token,
                Platform = platform
            });
            await db.SaveChangesAsync();
            return Ok(ApiResponse<bool>.Ok(true, "Device registered."));
        }

        public class RegisterDeviceRequest
        {
            public string Token { get; set; } = "";
            public string Platform { get; set; } = "";   // Android | iOS
        }
    }
}
