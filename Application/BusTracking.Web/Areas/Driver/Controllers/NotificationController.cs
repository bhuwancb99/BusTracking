namespace BusTracking.Web.Areas.Driver.Controllers;

[Area("Driver"), Authorize(Roles = "Driver")]
public class NotificationController : Controller
{
    private readonly INotificationService _notif;

    private int UserId => int.TryParse(
        User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    public NotificationController(INotificationService notif) => _notif = notif;

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
