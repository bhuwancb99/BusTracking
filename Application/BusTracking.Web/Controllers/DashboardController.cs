using BusTracking.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusTracking.Web.Controllers
{
    [Authorize]
    public class DashboardController : BaseController
    {
        private readonly IDashboardService _dash;
        public DashboardController(IDashboardService dash) => _dash = dash;

        public async Task<IActionResult> Index()
        {
            var result = await _dash.GetSummaryAsync();
            return View(result.Data);
        }
    }
}
