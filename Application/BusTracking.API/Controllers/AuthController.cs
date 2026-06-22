namespace BusTracking.API.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : ApiBaseController
    {
        private readonly IAuthService _auth;
        private readonly IUserService _user;
        public AuthController(IAuthService auth, IUserService user) { _auth = auth; _user = user; }

        /// <summary>Login — works for all roles using Username + Password</summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var r = await _auth.LoginAsync(dto);
            return r.Success ? Ok(r) : Unauthorized(r);
        }

        /// <summary>Check if a username is available. Pass excludeUserId to allow editing own username.</summary>
        [HttpGet("check-username")]
        public async Task<IActionResult> CheckUsername([FromQuery] string userName, [FromQuery] int? excludeUserId = null)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return BadRequest(ApiResponse<bool>.Fail("Username is required."));
            var r = await _auth.CheckUsernameAsync(userName.Trim(), excludeUserId);
            return Ok(r);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var r = await _auth.ForgotPasswordAsync(dto);
            return Ok(r);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var r = await _auth.ResetPasswordAsync(dto);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [Authorize, HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var r = await _auth.ChangePasswordAsync(CurrentUserId, dto);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        /// <summary>Get current user profile — use after login to verify token</summary>
        [Authorize, HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var r = await _user.GetProfileAsync(CurrentUserId);
            return r.Success ? Ok(r) : NotFound(r);
        }
    }
}
