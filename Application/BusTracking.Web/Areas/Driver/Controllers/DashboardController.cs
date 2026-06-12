namespace BusTracking.Web.Areas.Driver.Controllers;

[Area("Driver"), Authorize(Roles = "Driver")]
public class DashboardController : Controller
{
    private int CurrentUserId => int.TryParse(
        User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    public IActionResult Index() => View();
}
