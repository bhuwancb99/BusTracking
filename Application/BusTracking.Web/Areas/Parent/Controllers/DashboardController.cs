using BusTracking.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BusTracking.Web.Areas.Parent.Controllers
{
    public class DashboardController : ParentBaseController
    {
        private readonly IStudentService _student;
        private readonly ITripService _trip;
        public DashboardController(IStudentService s, ITripService t) { _student = s; _trip = t; }

        public IActionResult Index() => View();
    }
}
