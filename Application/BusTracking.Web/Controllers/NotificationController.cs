using BusTracking.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusTracking.Web.Controllers
{
    [Authorize]
    public class NotificationController : BaseController
    {
        private readonly INotificationService _notif;
        public NotificationController(INotificationService notif) => _notif = notif;

        public async Task<IActionResult> Index()
        {
            var r = await _notif.GetUserNotificationsAsync(CurrentUserId);
            return View(r.Data);
        }

        [HttpPost]
        public async Task<IActionResult> MarkRead(int id)
        {
            await _notif.MarkAsReadAsync(id, CurrentUserId);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllRead()
        {
            await _notif.MarkAllAsReadAsync(CurrentUserId);
            return RedirectToAction(nameof(Index));
        }
    }
}
