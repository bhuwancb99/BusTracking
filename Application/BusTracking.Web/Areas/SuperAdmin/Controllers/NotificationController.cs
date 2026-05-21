namespace BusTracking.Web.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin"), Authorize(Roles = "SuperAdmin")]
    public class NotificationController : Controller
    {
        private readonly INotificationService _n;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        public NotificationController(INotificationService n) => _n = n;

        public async Task<IActionResult> Index()
        {
            var r = await _n.GetUserNotificationsAsync(UserId);
            return View(r.Data);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRead(int id)
        {
            await _n.MarkAsReadAsync(id, UserId);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllRead()
        {
            await _n.MarkAllAsReadAsync(UserId);
            return RedirectToAction(nameof(Index));
        }

    }
}
