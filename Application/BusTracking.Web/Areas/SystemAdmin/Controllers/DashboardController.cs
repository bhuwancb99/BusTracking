namespace BusTracking.Web.Areas.SystemAdmin.Controllers
{
    [Area("SystemAdmin")]
    [Authorize(Roles = "SystemAdmin")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _db;

        public DashboardController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            // Ignore tenant filters to see global numbers
            ViewBag.TotalSchools = await _db.Schools.IgnoreQueryFilters().CountAsync();
            ViewBag.TotalSuperAdmins = await _db.Users.IgnoreQueryFilters().CountAsync(u => u.RoleId == 1);
            ViewBag.TotalBuses = await _db.Buses.IgnoreQueryFilters().CountAsync();
            ViewBag.TotalStudents = await _db.Students.IgnoreQueryFilters().CountAsync();
            ViewBag.TotalLogs = await _db.Loggers.IgnoreQueryFilters().CountAsync();

            return View();
        }
    }
}
