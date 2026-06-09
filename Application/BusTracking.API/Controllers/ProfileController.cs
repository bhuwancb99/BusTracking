namespace BusTracking.API.Controllers
{
    [Authorize, Route("api/[controller]")]
    public class ProfileController : ApiBaseController
    {
        private readonly IUserService _user;
        private readonly AppDbContext _db;
        private readonly IImageService _img;

        public ProfileController(IUserService user, AppDbContext db, IImageService img)
        {
            _user = user; _db = db; _img = img;
        }

        // ── GET api/profile ───────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var r = await _user.GetProfileAsync(CurrentUserId);
            return r.Success ? Ok(r) : NotFound(r);
        }

        // ── PUT api/profile ───────────────────────────────────────────
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateProfileDto dto)
        {
            var r = await _user.UpdateProfileAsync(CurrentUserId, dto);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        // ── POST api/profile/photo ────────────────────────────────────
        /// <summary>
        /// Upload or replace own profile photo.
        /// Send as multipart/form-data with field name "file".
        /// Returns: { success, imageUrl }
        /// </summary>
        [HttpPost("photo")]
        [RequestSizeLimit(5_242_880)]
        public async Task<IActionResult> UploadPhoto(IFormFile file)
        {
            var user = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == CurrentUserId);

            if (user is null)
                return NotFound(ApiResponse<string>.Fail("User not found."));

            try
            {
                var folder = RoleToFolder(CurrentUserRole);
                var url = await _img.SaveProfileImageAsync(
                    file, CurrentUserId, folder, user.ProfileImageUrl);

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

        // ── DELETE api/profile/photo ──────────────────────────────────
        /// <summary>Remove own profile photo.</summary>
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

        // ── Helper ────────────────────────────────────────────────────
        private static string RoleToFolder(string role) => role.ToLower() switch
        {
            "superadmin" => "superadmin",
            "buscoordinator" => "coordinator",
            "driver" => "driver",
            "student" => "student",
            "parent" => "parent",
            _ => "users"
        };
    }
}
