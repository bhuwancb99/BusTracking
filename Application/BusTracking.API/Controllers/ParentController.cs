namespace BusTracking.API.Controllers
{
    [Authorize(Roles = "Parent"), Route("api/parent")]
    public class ParentController : ApiBaseController
    {
        private readonly AppDbContext _db;
        private readonly IImageService _img;
        private readonly IAcademicYearService _academicYearService;

        public ParentController(AppDbContext db, IImageService img, IAcademicYearService academicYearService)
        {
            _db = db;
            _img = img;
            _academicYearService = academicYearService;
        }

        /// <summary>
        /// Switch active academic session for Parent's school.
        /// </summary>
        [HttpPost("session/switch/{id:int}")]
        public async Task<IActionResult> SwitchSession(int id)
        {
            int schoolId = CurrentSchoolId ?? 1;
            var userName = User.Identity?.Name ?? User.GetFullName() ?? "Parent";
            var result = await _academicYearService.SetActiveAcademicYearAsync(schoolId, id, userName);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Gets active academic years for Parent.
        /// </summary>
        [HttpGet("academicyears")]
        public async Task<IActionResult> GetAcademicYears()
        {
            var schoolId = CurrentSchoolId ?? 1;
            var years = await _academicYearService.GetAcademicYearsAsync(schoolId);
            return Ok(ApiResponse<List<AcademicYearDto>>.Ok(years));
        }


        // ── GET api/parent/dashboard ──────────────────────────────────
        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var parent = await _db.Parents
                .IgnoreQueryFilters()
                .Include(p => p.ParentStudents)
                    .ThenInclude(ps => ps.Student).ThenInclude(s => s.User)
                .Include(p => p.ParentStudents)
                    .ThenInclude(ps => ps.Student).ThenInclude(s => s.Standard)
                .Include(p => p.ParentStudents)
                    .ThenInclude(ps => ps.Student).ThenInclude(s => s.Bus)
                .Include(p => p.ParentStudents)
                    .ThenInclude(ps => ps.Student).ThenInclude(s => s.Stop)
                .FirstOrDefaultAsync(p => p.UserId == CurrentUserId);

            if (parent is null)
                return NotFound(ApiResponse<object>.Fail("Parent record not found."));

            var children = parent.ParentStudents
                .Where(ps => ps.Student != null)
                .Select(ps => new
                {
                    ps.Student.StudentId,
                    ps.Student.UserId,
                    ps.Student.StudentCode,
                    FullName = ps.Student.User != null ? ps.Student.User.FullName : "Student",
                    StandardName = ps.Student.Standard != null ? ps.Student.Standard.StandardName : "N/A",
                    BusName = ps.Student.Bus != null ? ps.Student.Bus.BusName : "Not Assigned",
                    BusNumber = ps.Student.Bus != null ? ps.Student.Bus.BusNumber : "N/A",
                    StopName = ps.Student.Stop != null ? ps.Student.Stop.StopName : "Not Assigned"
                })
                .ToList();

            return Ok(ApiResponse<object>.Ok(new
            {
                ParentId = parent.ParentId,
                ChildrenCount = children.Count,
                Children = children
            }));
        }

        // ── POST api/parent/photo ─────────────────────────────────────
        [HttpPost("photo")]
        [RequestSizeLimit(5_242_880)]
        public async Task<IActionResult> UploadPhoto(IFormFile file)
        {
            var user = await _db.Users.FindAsync(CurrentUserId);
            if (user is null)
                return NotFound(ApiResponse<string>.Fail("User not found."));

            try
            {
                var url = await _img.SaveProfileImageAsync(
                    file, CurrentUserId, "parent", user.ProfileImageUrl);

                user.ProfileImageUrl = url;
                user.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                return Ok(ApiResponse<string>.Ok(url, "Profile photo updated."));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message));
            }
        }

        // ── DELETE api/parent/photo ───────────────────────────────────
        [HttpDelete("photo")]
        public async Task<IActionResult> DeletePhoto()
        {
            var user = await _db.Users.FindAsync(CurrentUserId);
            if (user is null)
                return NotFound(ApiResponse<bool>.Fail("User not found."));

            _img.DeleteFile(user.ProfileImageUrl);
            user.ProfileImageUrl = null;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok(ApiResponse<bool>.Ok(true, "Profile photo removed."));
        }
    }
}
