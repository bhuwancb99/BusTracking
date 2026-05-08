using BusTracking.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BusTracking.Web.Areas.SuperAdmin.Controllers
{
    public class NotificationController : SuperAdminBaseController
    {
        private readonly INotificationService _notif;
        public NotificationController(INotificationService n) => _notif = n;

        public async Task<IActionResult> Index()
        {
            var r = await _notif.GetUserNotificationsAsync(CurrentUserId);
            return View(r.Data);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRead(int id)
        {
            await _notif.MarkAsReadAsync(id, CurrentUserId);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllRead()
        {
            await _notif.MarkAllAsReadAsync(CurrentUserId);
            return RedirectToAction(nameof(Index));
        }
    }
}
