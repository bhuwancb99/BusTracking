using Microsoft.AspNetCore.Mvc;

namespace BusTracking.Web.Areas.Student.Controllers
{
    public class DashboardController : StudentBaseController
    {
        public IActionResult Index() => View();
    }
}
