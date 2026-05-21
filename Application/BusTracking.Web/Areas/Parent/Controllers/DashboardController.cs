namespace BusTracking.Web.Areas.Parent.Controllers
{
    [Area("Parent"), Authorize(Roles = "Parent")]
    public class DashboardController : Controller
    {
        public IActionResult Index() => View();
    }
}
