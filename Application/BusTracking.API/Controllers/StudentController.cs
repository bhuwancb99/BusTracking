namespace BusTracking.API.Controllers
{
    /// <summary>API for Student MAUI app. All endpoints require role = Student.</summary>
    [Authorize(Roles = "Student"), Route("api/student")]
    public class StudentController : ApiBaseController
    {
        private readonly AppDbContext    _db;
        private readonly IStudentService _student;
        public StudentController(AppDbContext db, IStudentService student)
        {
            _db = db; _student = student;
        }

        // GET api/student/dashboard
        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var student = await _db.Students
                .Include(s => s.User)
                .Include(s => s.Bus)
                .Include(s => s.Stop)
                .FirstOrDefaultAsync(s => s.UserId == CurrentUserId);

            if (student is null)
                return NotFound(ApiResponse<object>.Fail("Student record not found."));

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var trip = student.BusId is null ? null : await _db.BusTrips
                .FirstOrDefaultAsync(t => t.BusId == student.BusId
                                       && t.TripDate == today
                                       && t.Status == TripStatus.InProgress);

            var boarding = trip is null ? null : await _db.StudentTripStatuses
                .FirstOrDefaultAsync(s => s.TripId == trip.TripId && s.StudentId == student.StudentId);

            return Ok(ApiResponse<object>.Ok(new
            {
                student.StudentCode,
                student.Standard,
                Bus = student.Bus is null ? null : new
                {
                    student.Bus.BusId,
                    student.Bus.BusName,
                    student.Bus.BusNumber
                },
                Stop = student.Stop is null ? null : new
                {
                    student.Stop.StopId,
                    student.Stop.StopName,
                    student.Stop.Latitude,
                    student.Stop.Longitude
                },
                ActiveTrip = trip is null ? null : new
                {
                    trip.TripId,
                    TripType       = trip.TripType.ToString(),
                    Status         = trip.Status.ToString(),
                    BoardingStatus = boarding?.BoardingStatus.ToString() ?? "Pending"
                }
            }));
        }

        // GET api/student/track
        [HttpGet("track")]
        public async Task<IActionResult> TrackBus()
        {
            var student = await _db.Students
                .Include(s => s.Bus)
                .FirstOrDefaultAsync(s => s.UserId == CurrentUserId);

            if (student?.BusId is null)
                return NotFound(ApiResponse<object>.Fail("No bus assigned."));

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var trip = await _db.BusTrips
                .FirstOrDefaultAsync(t => t.BusId == student.BusId
                                       && t.TripDate == today
                                       && t.Status == TripStatus.InProgress);

            if (trip is null)
                return Ok(ApiResponse<object>.Ok(new
                {
                    IsLive  = false,
                    Message = "No active trip right now.",
                    Bus     = new { student.Bus!.BusName, student.Bus.BusNumber }
                }));

            var loc = await _db.BusLiveLocations
                .Where(l => l.TripId == trip.TripId)
                .OrderByDescending(l => l.RecordedAt)
                .Select(l => new { l.Latitude, l.Longitude, l.Speed, l.Heading, l.RecordedAt })
                .FirstOrDefaultAsync();

            var boarding = await _db.StudentTripStatuses
                .FirstOrDefaultAsync(s => s.TripId == trip.TripId && s.StudentId == student.StudentId);

            var stops = await _db.TripStopEvents
                .Include(e => e.Stop)
                .Where(e => e.TripId == trip.TripId)
                .OrderBy(e => e.Stop.StopOrder)
                .Select(e => new
                {
                    e.Stop.StopId,
                    e.Stop.StopName,
                    e.Stop.StopOrder,
                    e.Stop.Latitude,
                    e.Stop.Longitude,
                    Status     = e.Status.ToString(),
                    e.ReachedAt,
                    e.DepartedAt
                })
                .ToListAsync();

            return Ok(ApiResponse<object>.Ok(new
            {
                IsLive         = true,
                Trip           = new { trip.TripId, TripType = trip.TripType.ToString() },
                Bus            = new { student.Bus!.BusName, student.Bus.BusNumber },
                Location       = loc,
                BoardingStatus = boarding?.BoardingStatus.ToString() ?? "Pending",
                Stops          = stops
            }));
        }

        // GET api/student/availability?days=30
        [HttpGet("availability")]
        public async Task<IActionResult> GetAvailability([FromQuery] int days = 30)
        {
            var student = await _db.Students.FirstOrDefaultAsync(s => s.UserId == CurrentUserId);
            if (student is null) return NotFound(ApiResponse<object>.Fail("Student not found."));

            var from = DateOnly.FromDateTime(DateTime.UtcNow);
            var to   = from.AddDays(days);

            var avail = await _db.StudentAvailabilities
                .Where(a => a.StudentId == student.StudentId
                         && a.FromDate >= from && a.FromDate <= to)
                .OrderBy(a => a.FromDate)
                .Select(a => new
                {
                    a.AvailabilityId,
                    a.AvailabilityType,
                    a.FromDate,
                    a.ToDate,
                    a.Remarks
                })
                .ToListAsync();

            return Ok(ApiResponse<object>.Ok(new { StudentId = student.StudentId, Availability = avail }));
        }

        // POST api/student/availability
        [HttpPost("availability")]
        public async Task<IActionResult> SetAvailability([FromBody] CreateAvailabilityDto dto)
        {
            var student = await _db.Students.FirstOrDefaultAsync(s => s.UserId == CurrentUserId);
            if (student is null) return NotFound(ApiResponse<object>.Fail("Student not found."));

            dto.StudentId = student.StudentId;
            var r = await _student.SetAvailabilityAsync(dto, CurrentUserId);
            return r.Success ? Ok(r) : BadRequest(r);
        }
    }
}
