namespace BusTracking.API.Controllers
{
    /// <summary>
    /// API for Bus Coordinator MAUI app.
    /// All endpoints require role = BusCoordinator.
    /// </summary>
    [Authorize(Roles = "BusCoordinator"), Route("api/coordinator")]
    public class CoordinatorController : ApiBaseController
    {
        private readonly AppDbContext _db;
        private readonly IDashboardService _dash;
        private readonly ITripService _trip;
        public CoordinatorController(AppDbContext db, IDashboardService dash, ITripService trip)
        {
            _db = db; _dash = dash; _trip = trip;
        }

        // ══════════════════════════════════════════════════════════
        // DASHBOARD
        // ══════════════════════════════════════════════════════════

        // ── GET api/coordinator/dashboard ────────────────────────
        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var r = await _dash.GetSummaryAsync();
            return Ok(r);
        }

        // ══════════════════════════════════════════════════════════
        // TRIPS
        // ══════════════════════════════════════════════════════════

        // ── GET api/coordinator/trips ────────────────────────────
        [HttpGet("trips")]
        public async Task<IActionResult> Trips([FromQuery] string? status, [FromQuery] string? date)
        {
            var q = _db.BusTrips
                .Include(t => t.Bus)
                .Include(t => t.Driver)   // Driver is User directly — no ThenInclude needed
                .Include(t => t.Route)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<TripStatus>(status, true, out var s))
                q = q.Where(t => t.Status == s);

            if (DateOnly.TryParse(date, out var d))
                q = q.Where(t => t.TripDate == d);
            else
                q = q.Where(t => t.TripDate == DateOnly.FromDateTime(DateTime.UtcNow));

            var trips = await q.OrderByDescending(t => t.TripDate).Take(50)
                .Select(t => new
                {
                    t.TripId,
                    t.TripDate,
                    TripType = t.TripType.ToString(),
                    Status = t.Status.ToString(),
                    BusNumber = t.Bus.BusNumber,
                    BusName = t.Bus.BusName,
                    DriverName = t.Driver != null ? t.Driver.FullName : null,
                    RouteName = t.Route != null ? t.Route.RouteName : null,
                    t.StartedAt,
                    t.EndedAt
                }).ToListAsync();

            return Ok(ApiResponse<object>.Ok(trips));
        }

        // ── GET api/coordinator/trips/{tripId} ───────────────────
        [HttpGet("trips/{tripId}")]
        public async Task<IActionResult> TripDetail(int tripId)
        {
            var r = await _trip.GetByIdAsync(tripId);
            return r.Success ? Ok(r) : NotFound(r);
        }

        // ── POST api/coordinator/trips ───────────────────────────
        [HttpPost("trips")]
        public async Task<IActionResult> CreateTrip([FromBody] CreateTripRequest req)
        {
            if (!Enum.TryParse<TripType>(req.TripType, true, out _))
                return BadRequest(ApiResponse<bool>.Fail("Invalid trip type. Use Morning or Evening."));

            var dto = new CreateTripDto
            {
                BusId = req.BusId,
                RouteId = req.RouteId,
                TripType = req.TripType,
                TripDate = (req.TripDate ?? DateTime.UtcNow).ToString("yyyy-MM-dd")
            };

            var r = await _trip.CreateAsync(dto, CurrentUserId);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        // ── POST api/coordinator/trips/{tripId}/start ────────────
        [HttpPost("trips/{tripId}/start")]
        public async Task<IActionResult> StartTrip(int tripId)
        {
            var trip = await _db.BusTrips.FindAsync(tripId);
            if (trip is null) return NotFound(ApiResponse<bool>.Fail("Trip not found."));
            trip.Status = TripStatus.InProgress;
            trip.StartedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(ApiResponse<bool>.Ok(true, "Trip started."));
        }

        // ── POST api/coordinator/trips/{tripId}/end ──────────────
        [HttpPost("trips/{tripId}/end")]
        public async Task<IActionResult> EndTrip(int tripId)
        {
            var trip = await _db.BusTrips.FindAsync(tripId);
            if (trip is null) return NotFound(ApiResponse<bool>.Fail("Trip not found."));
            trip.Status = TripStatus.Completed;
            trip.EndedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(ApiResponse<bool>.Ok(true, "Trip completed."));
        }

        // ── POST api/coordinator/trips/{tripId}/cancel ───────────
        [HttpPost("trips/{tripId}/cancel")]
        public async Task<IActionResult> CancelTrip(int tripId)
        {
            var trip = await _db.BusTrips.FindAsync(tripId);
            if (trip is null) return NotFound(ApiResponse<bool>.Fail("Trip not found."));
            trip.Status = TripStatus.Cancelled;
            await _db.SaveChangesAsync();
            return Ok(ApiResponse<bool>.Ok(true, "Trip cancelled."));
        }

        // ── GET api/coordinator/trips/{tripId}/location ──────────
        [HttpGet("trips/{tripId}/location")]
        public async Task<IActionResult> TripLocation(int tripId)
        {
            var loc = await _db.BusLiveLocations
                .Where(l => l.TripId == tripId)
                .OrderByDescending(l => l.RecordedAt)
                .Select(l => new { l.Latitude, l.Longitude, l.Speed, l.Heading, l.RecordedAt })
                .FirstOrDefaultAsync();

            if (loc is null) return NotFound(ApiResponse<object>.Fail("No location data yet."));
            return Ok(ApiResponse<object>.Ok(loc));
        }

        // ── GET api/coordinator/trips/{tripId}/location/history ──
        [HttpGet("trips/{tripId}/location/history")]
        public async Task<IActionResult> LocationHistory(int tripId)
        {
            var history = await _db.BusLiveLocations
                .Where(l => l.TripId == tripId)
                .OrderBy(l => l.RecordedAt)
                .Select(l => new { l.Latitude, l.Longitude, l.Speed, l.Heading, l.RecordedAt })
                .ToListAsync();

            return Ok(ApiResponse<object>.Ok(history));
        }

        // ══════════════════════════════════════════════════════════
        // BUSES
        // ══════════════════════════════════════════════════════════

        // ── GET api/coordinator/buses ────────────────────────────
        [HttpGet("buses")]
        public async Task<IActionResult> Buses([FromQuery] string? search)
        {
            var q = _db.Buses.Include(b => b.Route).Include(b => b.Driver).ThenInclude(d => d!.User).AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(b => b.BusName.Contains(search) || b.BusNumber.Contains(search));

            var buses = await q.Where(b => b.IsActive).OrderBy(b => b.BusName)
                .Select(b => new
                {
                    b.BusId,
                    b.BusName,
                    b.BusNumber,
                    b.Capacity,
                    RouteName = b.Route != null ? b.Route.RouteName : null,
                    DriverName = b.Driver != null ? b.Driver.User.FullName : null,
                    b.IsActive
                }).ToListAsync();

            return Ok(ApiResponse<object>.Ok(buses));
        }

        // ── GET api/coordinator/buses/{busId} ────────────────────
        [HttpGet("buses/{busId}")]
        public async Task<IActionResult> BusDetail(int busId)
        {
            var b = await _db.Buses
                .Include(x => x.Route).ThenInclude(r => r!.Stops)
                .Include(x => x.Driver).ThenInclude(d => d!.User)
                .Include(x => x.Students).ThenInclude(s => s.User)
                .FirstOrDefaultAsync(x => x.BusId == busId);

            if (b is null) return NotFound(ApiResponse<object>.Fail("Bus not found."));

            return Ok(ApiResponse<object>.Ok(new
            {
                b.BusId,
                b.BusName,
                b.BusNumber,
                b.Capacity,
                b.IsActive,
                Route = b.Route is null ? null : new { b.Route.RouteId, b.Route.RouteName },
                Driver = b.Driver is null ? null : new { b.Driver.UserId, b.Driver.User.FullName, b.Driver.User.PhoneNumber },
                StudentCount = b.Students.Count
            }));
        }

        // ══════════════════════════════════════════════════════════
        // ROUTES
        // ══════════════════════════════════════════════════════════

        // ── GET api/coordinator/routes ───────────────────────────
        [HttpGet("routes")]
        public async Task<IActionResult> Routes()
        {
            var routes = await _db.Routes
                .Include(r => r.Stops)
                .Where(r => r.IsActive)
                .OrderBy(r => r.RouteName)
                .Select(r => new
                {
                    r.RouteId,
                    r.RouteName,
                    r.RouteCode,
                    r.MorningTime,
                    r.EveningTime,
                    StopCount = r.Stops.Count,
                    r.IsActive
                }).ToListAsync();

            return Ok(ApiResponse<object>.Ok(routes));
        }

        // ── GET api/coordinator/routes/{routeId}/stops ───────────
        [HttpGet("routes/{routeId}/stops")]
        public async Task<IActionResult> RouteStops(int routeId)
        {
            var stops = await _db.Stops
                .Where(s => s.RouteId == routeId && s.IsActive)
                .OrderBy(s => s.StopOrder)
                .Select(s => new { s.StopId, s.StopName, s.StopOrder, s.Latitude, s.Longitude })
                .ToListAsync();

            return Ok(ApiResponse<object>.Ok(stops));
        }

        // ══════════════════════════════════════════════════════════
        // DRIVERS
        // ══════════════════════════════════════════════════════════

        // ── GET api/coordinator/drivers ──────────────────────────
        [HttpGet("drivers")]
        public async Task<IActionResult> Drivers([FromQuery] string? search)
        {
            var roleId = await _db.Roles.Where(r => r.RoleName == "Driver").Select(r => r.RoleId).FirstAsync();
            var q = _db.Users
                .Include(u => u.DriverDetail).ThenInclude(d => d!.Bus)
                .Where(u => u.RoleId == roleId && u.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));

            var drivers = await q.OrderBy(u => u.FullName).Select(u => new
            {
                u.UserId,
                u.FullName,
                u.Email,
                u.PhoneNumber,
                BusNumber = u.DriverDetail != null && u.DriverDetail.Bus != null ? u.DriverDetail.Bus.BusNumber : null,
                BusName = u.DriverDetail != null && u.DriverDetail.Bus != null ? u.DriverDetail.Bus.BusName : null,
                LicenseNumber = u.DriverDetail != null ? u.DriverDetail.LicenseNumber : null,
                u.IsActive
            }).ToListAsync();

            return Ok(ApiResponse<object>.Ok(drivers));
        }

        // ══════════════════════════════════════════════════════════
        // PARENTS
        // ══════════════════════════════════════════════════════════

        // ── GET api/coordinator/parents ──────────────────────────
        [HttpGet("parents")]
        public async Task<IActionResult> Parents([FromQuery] string? search, [FromQuery] int page = 1)
        {
            var roleId = await _db.Roles.Where(r => r.RoleName == "Parent").Select(r => r.RoleId).FirstAsync();
            var q = _db.Users.Include(u => u.ParentDetail)
                    .ThenInclude(p => p!.ParentStudents).ThenInclude(ps => ps.Student)
                .Where(u => u.RoleId == roleId);

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));

            var parents = await q.OrderBy(u => u.FullName).Skip((page - 1) * 20).Take(20)
                .Select(u => new
                {
                    u.UserId,
                    u.FullName,
                    u.Email,
                    u.PhoneNumber,
                    u.IsActive,
                    ChildrenCount = u.ParentDetail != null ? u.ParentDetail.ParentStudents.Count : 0
                }).ToListAsync();

            return Ok(ApiResponse<object>.Ok(parents));
        }

        // ══════════════════════════════════════════════════════════
        // STUDENTS
        // ══════════════════════════════════════════════════════════

        // ── GET api/coordinator/students ─────────────────────────
        [HttpGet("students")]
        public async Task<IActionResult> Students([FromQuery] string? search, [FromQuery] int page = 1)
        {
            var q = _db.Students
                .Include(s => s.User)
                .Include(s => s.Bus)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(s => s.User.FullName.Contains(search) || s.StudentCode.Contains(search));

            var students = await q.Where(s => s.User.IsActive)
                .OrderBy(s => s.User.FullName).Skip((page - 1) * 20).Take(20)
                .Select(s => new
                {
                    s.StudentId,
                    s.StudentCode,
                    FullName = s.User.FullName,
                    s.Standard,
                    BusNumber = s.Bus != null ? s.Bus.BusNumber : null,
                    BusName = s.Bus != null ? s.Bus.BusName : null,
                    s.User.IsActive
                }).ToListAsync();

            return Ok(ApiResponse<object>.Ok(students));
        }

        // ══════════════════════════════════════════════════════════
        // REQUEST MODELS
        // ══════════════════════════════════════════════════════════
        public class CreateTripRequest
        {
            public int BusId { get; set; }
            public int RouteId { get; set; }
            public string TripType { get; set; } = "Morning";
            public DateTime? TripDate { get; set; }
        }
    }
}