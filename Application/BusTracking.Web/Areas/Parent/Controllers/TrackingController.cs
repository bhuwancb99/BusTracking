namespace BusTracking.Web.Areas.Parent.Controllers
{
    [Area("Parent"), Authorize(Roles = "Parent")]
    public class TrackingController : Controller
    {
        private readonly IParentService _parentService;
        private readonly ITripService _tripService;
        private readonly IAppConfigService _appConfig;
        private readonly AppDbContext _db;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        public TrackingController(
            IParentService parentService,
            ITripService tripService,
            IAppConfigService appConfig,
            AppDbContext db)
        {
            _parentService = parentService;
            _tripService = tripService;
            _appConfig = appConfig;
            _db = db;
        }

        public async Task<IActionResult> Track(int studentId = 0)
        {
            var parentRes = await _parentService.GetByIdAsync(UserId);
            var studentIds = parentRes.Success && parentRes.Data != null
                ? parentRes.Data.Students.Select(s => s.StudentId).ToList()
                : new List<int>();

            var children = await _db.Students
                .Include(s => s.User)
                .Include(s => s.Bus)
                .Include(s => s.Stop)
                .Include(s => s.Standard)
                .Where(s => studentIds.Contains(s.StudentId) && s.User.IsActive)
                .ToListAsync();

            ViewBag.Children = children;

            var selectedStudent = (studentId > 0 ? children.FirstOrDefault(c => c.StudentId == studentId) : null)
                                ?? children.FirstOrDefault();

            ViewBag.SelectedStudent = selectedStudent;
            ViewBag.GoogleMapApiKey = await _appConfig.GetValueAsync("GoogleMapApiKey") ?? "";
            var rawHubUrl = await _appConfig.GetValueAsync(AppConstants.AppConfigTrackingHubUrlKey);
            ViewBag.TrackingHubUrl = AppConstants.FormatTrackingHubUrl(rawHubUrl);

            if (selectedStudent?.BusId == null)
            {
                ViewBag.Trip = null;
                return View(new List<StudentTripStatusDto>());
            }

            var busId = selectedStudent.BusId.Value;
            var bus = await _db.Buses.FirstOrDefaultAsync(b => b.BusId == busId);
            var school = bus?.SchoolId.HasValue == true
                ? await _db.Schools.Include(s => s.TimeZone).FirstOrDefaultAsync(s => s.SchoolId == bus.SchoolId.Value)
                : null;
            var today = TimeZoneHelper.GetSchoolTodayDate(school);

            var trip = await _db.BusTrips
                .Include(t => t.Bus)
                .Include(t => t.Driver)
                .Include(t => t.Route)
                .FirstOrDefaultAsync(t => t.BusId == busId && t.Status == TripStatus.InProgress)
                ?? await _db.BusTrips
                .Include(t => t.Bus)
                .Include(t => t.Driver)
                .Include(t => t.Route)
                .FirstOrDefaultAsync(t => t.BusId == busId && t.TripDate == today && t.Status != TripStatus.Cancelled);

            ViewBag.Trip = trip;

            if (trip != null)
            {
                var students = await _tripService.GetTripStudentsAsync(trip.TripId);
                var stops = await _tripService.GetStopEventsAsync(trip.TripId);
                var location = await _tripService.GetLatestLocationAsync(trip.TripId);

                ViewBag.TripId = trip.TripId;
                ViewBag.Stops = stops.Data ?? [];
                ViewBag.Location = location.Data;
                return View(students.Data ?? []);
            }

            ViewBag.TripId = 0;
            return View(new List<StudentTripStatusDto>());
        }

        [HttpGet]
        public async Task<IActionResult> LatestLocation(int tripId)
        {
            var r = await _tripService.GetLatestLocationAsync(tripId);
            return Json(new { success = r.Success, data = r.Data });
        }

        [HttpGet]
        public async Task<IActionResult> LocationHistory(int tripId)
        {
            var r = await _tripService.GetLocationHistoryAsync(tripId);
            return Json(new { success = r.Success, data = r.Data });
        }
    }
}
