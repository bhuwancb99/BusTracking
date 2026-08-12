namespace BusTracking.API.Controllers
{
    /// <summary>
    /// All endpoints here are for the Driver role only.
    /// Route: /api/driver/...
    /// </summary>
    [Authorize(Roles = "Driver"), Route("api/driver")]
    public class DriverController : ApiBaseController
    {
        private readonly AppDbContext _db;
        private readonly INotificationService _notif;
        private readonly IDriverTripWebService _driverTrip;
        private readonly IAcademicYearService _academicYearService;

        public DriverController(
            AppDbContext db,
            INotificationService notif,
            IDriverTripWebService driverTrip,
            IAcademicYearService academicYearService)
        {
            _db = db;
            _notif = notif;
            _driverTrip = driverTrip;
            _academicYearService = academicYearService;
        }

        /// <summary>
        /// Switch active academic session for Driver's school.
        /// </summary>
        [HttpPost("session/switch/{id:int}")]
        public async Task<IActionResult> SwitchSession(int id)
        {
            int schoolId = CurrentSchoolId ?? 1;
            var userName = User.Identity?.Name ?? User.GetFullName() ?? "Driver";
            var result = await _academicYearService.SetActiveAcademicYearAsync(schoolId, id, userName);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Gets active academic years for Driver.
        /// </summary>
        [HttpGet("academicyears")]
        public async Task<IActionResult> GetAcademicYears()
        {
            var schoolId = CurrentSchoolId ?? 1;
            var years = await _academicYearService.GetAcademicYearsAsync(schoolId);
            return Ok(ApiResponse<List<AcademicYearDto>>.Ok(years));
        }


        // ══════════════════════════════════════════════════════════════════
        // DASHBOARD
        // ══════════════════════════════════════════════════════════════════

        /// <summary>
        /// GET /api/driver/dashboard
        /// Returns the driver's assigned bus, route, and today's trip summary.
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var driver = await _db.DriverDetails
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == CurrentUserId);

            if (driver is null)
                return NotFound(ApiResponse<object>.Fail("Driver record not found."));

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var trips = await _db.BusTrips
                .Include(t => t.Route)
                .Where(t => t.DriverId == driver.DriverDetailId && t.TripDate == today)
                .Select(t => new
                {
                    t.TripId,
                    TripType = t.TripType.ToString(),
                    Status = t.Status.ToString(),
                    RouteName = t.Route != null ? t.Route.RouteName : "Unassigned"
                })
                .ToListAsync();

            return Ok(ApiResponse<object>.Ok(new
            {
                DriverCode = $"DRV-{driver.DriverDetailId:D4}",
                FullName = driver.User?.FullName ?? "Driver",
                BusName = "Assigned Bus",
                BusNumber = "N/A",
                TodayTripsCount = trips.Count,
                Trips = trips
            }));
        }
    }
}
