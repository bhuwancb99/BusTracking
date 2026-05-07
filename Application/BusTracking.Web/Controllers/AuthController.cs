using BusTracking.Common.DTOs.Auth;
using BusTracking.Common.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BusTracking.Web.Controllers
{
    public class AuthController : BaseController
    {
        private readonly IAuthService _auth;
        public AuthController(IAuthService auth) => _auth = auth;

        // GET /Auth/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST /Auth/Login
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto model, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _auth.LoginAsync(model);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View(model);
            }

            var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.Data!.UserId.ToString()),
            new(ClaimTypes.Email,          result.Data.Email),
            new(ClaimTypes.Name,           result.Data.FullName),
            new(ClaimTypes.Role,           result.Data.Role),
            new("JwtToken",                result.Data.Token)
        };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
                new AuthenticationProperties { IsPersistent = true, ExpiresUtc = result.Data.Expiry });

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Dashboard");
        }

        // GET /Auth/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword() => View();

        // POST /Auth/ForgotPassword
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto model)
        {
            if (!ModelState.IsValid) return View(model);
            var result = await _auth.ForgotPasswordAsync(model);
            TempData["Message"] = result.Message;
            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        // GET /Auth/ForgotPasswordConfirmation
        public IActionResult ForgotPasswordConfirmation() => View();

        // GET /Auth/ResetPassword
        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            ViewBag.Token = token;
            return View();
        }

        // POST /Auth/ResetPassword
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
        {
            if (!ModelState.IsValid) return View(model);
            var result = await _auth.ResetPasswordAsync(model);
            if (!result.Success) { ModelState.AddModelError("", result.Message); return View(model); }
            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Login));
        }

        // POST /Auth/Logout
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        // GET /Auth/AccessDenied
        public IActionResult AccessDenied() => View();

        // GET /Auth/ChangePassword
        [Authorize]
        public IActionResult ChangePassword() => View();

        // POST /Auth/ChangePassword
        [Authorize, HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto model)
        {
            if (!ModelState.IsValid) return View(model);
            var result = await _auth.ChangePasswordAsync(CurrentUserId, model);
            if (!result.Success) { ModelState.AddModelError("", result.Message); return View(model); }
            TempData["SuccessMessage"] = "Password changed successfully.";
            return RedirectToAction("Index", "Profile");
        }
    }
}
