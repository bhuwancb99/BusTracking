namespace BusTracking.Web.Areas.BusCoordinator.Controllers
{
    // ── Dashboard ─────────────────────────────────────────────────
    [Area("BusCoordinator"), Authorize(Roles = "BusCoordinator")]
    public class DashboardController : Controller
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
