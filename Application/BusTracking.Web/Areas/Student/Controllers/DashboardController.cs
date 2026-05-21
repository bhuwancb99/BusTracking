namespace BusTracking.Web.Areas.Student.Controllers
{
    [Area("Student"), Authorize(Roles = "Student")]
    public class DashboardController : Controller
    {
        public IActionResult Index() => View();
    }
}
