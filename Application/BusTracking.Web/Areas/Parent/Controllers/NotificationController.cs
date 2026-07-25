namespace BusTracking.Web.Areas.Parent.Controllers
{
    [Area("Parent"), Authorize(Roles = "Parent")]
    public class NotificationController : Controller
    {
        private readonly INotificationService _n;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        public NotificationController(INotificationService n) => _n = n;

        public async Task<IActionResult> Index(int page = 1, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var today = DateTime.Today;
            var fDate = fromDate ?? today;
            var tDate = toDate ?? today;

            ViewBag.FromDate = fDate.ToString("yyyy-MM-dd");
            ViewBag.ToDate = tDate.ToString("yyyy-MM-dd");

            var r = await _n.GetUserNotificationsPagedAsync(UserId, page, fDate, tDate);
            return View(r.Data);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRead(int id, int page = 1, string? fromDate = null, string? toDate = null)
        {
            await _n.MarkAsReadAsync(id, UserId);
            return RedirectToAction(nameof(Index), new { page, fromDate, toDate });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllRead(int page = 1, string? fromDate = null, string? toDate = null)
        {
            await _n.MarkAllAsReadAsync(UserId);
            return RedirectToAction(nameof(Index), new { page, fromDate, toDate });
        }
    }
}
