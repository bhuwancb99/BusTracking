namespace BusTracking.Web.Areas.BusCoordinator.Controllers
{
    [Area("BusCoordinator"), Authorize(Roles = "BusCoordinator")]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notif;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        public NotificationController(INotificationService n) => _notif = n;

        public async Task<IActionResult> Index()
        {
            var r = await _notif.GetUserNotificationsAsync(UserId);
            return View(r.Data);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRead(int id)
        {
            await _notif.MarkAsReadAsync(id, UserId);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllRead()
        {
            await _notif.MarkAllAsReadAsync(UserId);
            return RedirectToAction(nameof(Index));
        }
    }
}
