using BusTracking.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BusTracking.Web.Areas.BusCoordinator.Controllers
{
    public class DashboardController : CoordBaseController
    {
        private readonly IDashboardService _dash;
        public DashboardController(IDashboardService d) => _dash = d;
        public async Task<IActionResult> Index()
        {
            var r = await _dash.GetSummaryAsync();
            return View(r.Data);
        }
    }
}
