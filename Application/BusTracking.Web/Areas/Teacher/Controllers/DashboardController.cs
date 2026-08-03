namespace BusTracking.Web.Areas.Teacher.Controllers
{
    [Area("Teacher"), Authorize(Roles = "Teacher")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ITeacherService _teacherService;

        private int CurrentUserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : 0;
        private int CurrentSchoolId => int.TryParse(User.FindFirst("SchoolId")?.Value, out var sid) ? sid : 0;

        public DashboardController(AppDbContext db, ITeacherService teacherService)
        {
            _db = db;
            _teacherService = teacherService;
        }

        public async Task<IActionResult> Index()
        {
            var profileResult = await _teacherService.GetTeacherByUserIdAsync(CurrentUserId);
            var teacher = profileResult.Data;

            // Fetch quick stats
            int totalNotifications = await _db.Notifications.CountAsync(n => n.RecipientUserId == CurrentUserId);
            int unreadNotifications = await _db.Notifications.CountAsync(n => n.RecipientUserId == CurrentUserId && !n.IsRead);

            ViewBag.Teacher = teacher;
            ViewBag.TotalNotifications = totalNotifications;
            ViewBag.UnreadNotifications = unreadNotifications;

            return View();
        }
    }
}
