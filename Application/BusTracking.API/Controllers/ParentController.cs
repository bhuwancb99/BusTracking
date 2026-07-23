namespace BusTracking.API.Controllers
{
    [Authorize(Roles = "Parent"), Route("api/parent")]
    public class ParentController : ApiBaseController
    {
        private readonly AppDbContext _db;
        private readonly IImageService _img;

        public ParentController(AppDbContext db, IImageService img)
        {
            _db = db; _img = img;
        }

        // ── GET api/parent/dashboard ──────────────────────────────────
        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var parent = await _db.Parents
                .Include(p => p.ParentStudents)
                    .ThenInclude(ps => ps.Student).ThenInclude(s => s.User)
                .Include(p => p.ParentStudents)
                    .ThenInclude(ps => ps.Student).ThenInclude(s => s.Bus)
                .Include(p => p.ParentStudents)
                    .ThenInclude(ps => ps.Student).ThenInclude(s => s.Stop)
                .FirstOrDefaultAsync(p => p.UserId == CurrentUserId);

            if (parent is null)
                return NotFound(ApiResponse<object>.Fail("Parent not found."));

            var children = parent.ParentStudents.Select(ps => new
            {
                ps.Student.StudentId,
                ps.Student.UserId,                              // ← needed for photo upload
                ps.Student.StudentCode,
                FullName = ps.Student.User.FullName,
                ps.Student.Standard,
                ProfileImageUrl = ps.Student.User.ProfileImageUrl,  // ← NEW
                Bus = ps.Student.Bus is null ? null : new
                {
                    ps.Student.Bus.BusId,
                    ps.Student.Bus.BusName,
                    ps.Student.Bus.BusNumber
                },
                Stop = ps.Student.Stop is null ? null : new
                {
                    ps.Student.Stop.StopId,
                    ps.Student.Stop.StopName,
                    ps.Student.Stop.Latitude,
                    ps.Student.Stop.Longitude
                }
            }).ToList();

            return Ok(ApiResponse<object>.Ok(new { Children = children }));
        }

        // ── POST api/parent/children/{studentId}/photo ────────────────
        /// <summary>
        /// Parent uploads/replaces a photo for their own child (student).
        /// Security: verifies the student is linked to this parent.
        /// Send as multipart/form-data, field name "file".
        /// </summary>
        [HttpPost("children/{studentId}/photo")]
        [RequestSizeLimit(5_242_880)]
        public async Task<IActionResult> UpdateChildPhoto(int studentId, IFormFile file)
        {
            // Verify this student belongs to the logged-in parent
            var parentDetail = await _db.Parents.FirstOrDefaultAsync(p => p.UserId == CurrentUserId);
            if (parentDetail is null) return Forbid();

            var link = await _db.ParentStudents
                .FirstOrDefaultAsync(ps => ps.StudentId == studentId
                                        && ps.ParentId == parentDetail.ParentId);
            if (link is null)
                return Forbid();

            var student = await _db.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);

            if (student is null)
                return NotFound(ApiResponse<string>.Fail("Student not found."));

            try
            {
                var url = await _img.SaveProfileImageAsync(
                    file, student.UserId, "student", student.User.ProfileImageUrl);

                student.User.ProfileImageUrl = url;
                student.User.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                return Ok(ApiResponse<string>.Ok(url, "Child photo updated."));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message));
            }
        }

        // ── DELETE api/parent/children/{studentId}/photo ──────────────
        [HttpDelete("children/{studentId}/photo")]
        public async Task<IActionResult> DeleteChildPhoto(int studentId)
        {
            var parentDetail = await _db.Parents.FirstOrDefaultAsync(p => p.UserId == CurrentUserId);
            if (parentDetail is null) return Forbid();

            var link = await _db.ParentStudents
                .FirstOrDefaultAsync(ps => ps.StudentId == studentId
                                        && ps.ParentId == parentDetail.ParentId);
            if (link is null) return Forbid();

            var student = await _db.Students.Include(s => s.User)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);
            if (student is null) return NotFound(ApiResponse<bool>.Fail("Student not found."));

            _img.DeleteFile(student.User.ProfileImageUrl);
            student.User.ProfileImageUrl = null;
            student.User.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok(ApiResponse<bool>.Ok(true, "Child photo removed."));
        }

        // ── GET api/parent/children/{studentId}/track ─────────────────
        [HttpGet("children/{studentId}/track")]
        public async Task<IActionResult> TrackBus(int studentId)
        {
            var parentDetail = await _db.Parents.FirstOrDefaultAsync(p => p.UserId == CurrentUserId);
            if (parentDetail is null) return Forbid();

            var link = await _db.ParentStudents
                .FirstOrDefaultAsync(ps => ps.StudentId == studentId
                                        && ps.ParentId == parentDetail.ParentId);
            if (link is null) return Forbid();

            var student = await _db.Students.Include(s => s.Bus)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);

            if (student?.BusId is null)
                return NotFound(ApiResponse<object>.Fail("No bus assigned to this student."));

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var trip = await _db.BusTrips
                .Include(t => t.Driver)
                .FirstOrDefaultAsync(t => t.BusId == student.BusId
                                       && t.TripDate == today
                                       && t.Status == TripStatus.InProgress);

            if (trip is null)
                return Ok(ApiResponse<object>.Ok(new
                {
                    IsLive = false,
                    Message = "No active trip right now.",
                    Bus = new { student.Bus!.BusName, student.Bus.BusNumber }
                }));

            var loc = await _db.BusLiveLocations
                .Where(l => l.TripId == trip.TripId)
                .OrderByDescending(l => l.RecordedAt)
                .Select(l => new { l.Latitude, l.Longitude, l.Speed, l.Heading, l.RecordedAt })
                .FirstOrDefaultAsync();

            var boarding = await _db.StudentTripStatuses
                .FirstOrDefaultAsync(s => s.TripId == trip.TripId && s.StudentId == studentId);

            return Ok(ApiResponse<object>.Ok(new
            {
                IsLive = true,
                Trip = new { 
                    trip.TripId, 
                    TripType = trip.TripType.ToString(), 
                    Status = trip.Status.ToString(),
                    DriverName = trip.Driver?.FullName ?? "Bus Driver"
                },
                Bus = new { student.Bus!.BusName, student.Bus.BusNumber },
                Location = loc,
                BoardingStatus = boarding?.BoardingStatus.ToString() ?? "Pending"
            }));
        }

        // ── GET api/parent/children/{studentId}/availability ──────────
        [HttpGet("children/{studentId}/availability")]
        public async Task<IActionResult> Availability(int studentId)
        {
            var parentDetail = await _db.Parents.FirstOrDefaultAsync(p => p.UserId == CurrentUserId);
            if (parentDetail is null) return Forbid();

            var link = await _db.ParentStudents
                .FirstOrDefaultAsync(ps => ps.StudentId == studentId
                                        && ps.ParentId == parentDetail.ParentId);
            if (link is null) return Forbid();

            var avail = await _db.StudentAvailabilities
                .Where(a => a.StudentId == studentId && a.FromDate >= DateOnly.FromDateTime(DateTime.UtcNow))
                .OrderBy(a => a.FromDate)
                .Select(a => new
                {
                    a.AvailabilityId,
                    a.AvailabilityType,
                    a.FromDate,
                    a.ToDate,
                    a.Remarks
                }).ToListAsync();

            return Ok(ApiResponse<object>.Ok(new { StudentId = studentId, Availability = avail }));
        }

        // ── GET api/parent/trips/history?days=7 ───────────────────────
        [HttpGet("trips/history")]
        public async Task<IActionResult> TripHistory([FromQuery] int days = 7)
        {
            var parentDetail = await _db.Parents.FirstOrDefaultAsync(p => p.UserId == CurrentUserId);
            if (parentDetail is null) return Forbid();

            var studentIds = await _db.ParentStudents
                .Where(ps => ps.ParentId == parentDetail.ParentId)
                .Select(ps => ps.StudentId).ToListAsync();

            var since = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-days));
            var trips = await _db.StudentTripStatuses
                .Include(s => s.Trip).ThenInclude(t => t.Bus)
                .Include(s => s.Student).ThenInclude(st => st.User)
                .Where(s => studentIds.Contains(s.StudentId) && s.Trip.TripDate >= since)
                .OrderByDescending(s => s.Trip.TripDate)
                .Select(s => new
                {
                    s.Trip.TripId,
                    s.Trip.TripDate,
                    TripType = s.Trip.TripType.ToString(),
                    BusNumber = s.Trip.Bus.BusNumber,
                    StudentName = s.Student.User.FullName,
                    BoardingStatus = s.BoardingStatus.ToString()
                })
                .ToListAsync();

            return Ok(ApiResponse<object>.Ok(trips));
        }
    }
}
