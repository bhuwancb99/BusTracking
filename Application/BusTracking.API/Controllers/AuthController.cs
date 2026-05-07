using BusTracking.Common.DTOs.Auth;
using BusTracking.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusTracking.API.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : ApiBaseController
    {
        private readonly IAuthService _auth;
        public AuthController(IAuthService auth) => _auth = auth;

        /// <summary>Driver login</summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var r = await _auth.LoginAsync(dto);
            return r.Success ? Ok(r) : Unauthorized(r);
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
    }
}
