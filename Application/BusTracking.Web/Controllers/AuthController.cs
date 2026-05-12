using BusTracking.Common.DTOs.Auth;
using BusTracking.Common.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BusTracking.Web.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _auth;
    public AuthController(IAuthService auth) => _auth = auth;

    private int CurrentUserId =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    // ── GET /Auth/Login ──────────────────────────────────────────────
    [HttpGet]
    public IActionResult Login()
    {
        // Already logged in → go straight to their area dashboard.
        // Do NOT read ReturnUrl here — that is what causes the loop.
        if (User.Identity?.IsAuthenticated == true)
            return DashboardRedirect(User.FindFirstValue(ClaimTypes.Role));

        return View(new LoginDto());
    }

    // ── POST /Auth/Login ─────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _auth.LoginAsync(model);
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Message);
            return View(model);
        }

        // Sign in
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.Data!.UserId.ToString()),
            new(ClaimTypes.Email,          result.Data.Email),
            new(ClaimTypes.Name,           result.Data.FullName),
            new(ClaimTypes.Role,           result.Data.Role),
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims,
                CookieAuthenticationDefaults.AuthenticationScheme)),
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8),
                AllowRefresh = true
            });

        // Always redirect to role dashboard — never follow ReturnUrl
        // (ReturnUrl from cookie auth is /Auth/Login which loops)
        return DashboardRedirect(result.Data.Role);
    }

    // ── POST /Auth/Logout ────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    // ── GET /Auth/ForgotPassword ─────────────────────────────────────
    [HttpGet] public IActionResult ForgotPassword() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto model)
    {
        if (!ModelState.IsValid) return View(model);
        var r = await _auth.ForgotPasswordAsync(model);
        TempData["Message"] = r.Message;
        return RedirectToAction(nameof(ForgotPasswordConfirmation));
    }

    [HttpGet] public IActionResult ForgotPasswordConfirmation() => View();

    // ── GET /Auth/ResetPassword ──────────────────────────────────────
    [HttpGet]
    public IActionResult ResetPassword(string token)
        => View(new ResetPasswordDto { Token = token });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
    {
        if (!ModelState.IsValid) return View(model);
        var r = await _auth.ResetPasswordAsync(model);
        if (!r.Success) { ModelState.AddModelError("", r.Message); return View(model); }
        TempData["SuccessMessage"] = r.Message;
        return RedirectToAction(nameof(Login));
    }

    // ── Change Password (any logged-in role) ─────────────────────────
    [Authorize, HttpGet]
    public IActionResult ChangePassword() => View();

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto model)
    {
        if (!ModelState.IsValid) return View(model);
        var r = await _auth.ChangePasswordAsync(CurrentUserId, model);
        if (!r.Success) { ModelState.AddModelError("", r.Message); return View(model); }
        TempData["SuccessMessage"] = "Password changed.";
        return RedirectToAction("Index", "Profile");
    }

    // ── Access Denied ────────────────────────────────────────────────
    [HttpGet] public IActionResult AccessDenied() => View();

    // ── Helper: role → absolute area path ───────────────────────────
    // Use absolute path strings — safest way to redirect into an Area
    // from a non-area controller. RedirectToAction({ area=... }) can
    // generate wrong URLs depending on current route context.
    private IActionResult DashboardRedirect(string? role) => role switch
    {
        "SuperAdmin" => Redirect("/SuperAdmin/Dashboard/Index"),
        "BusCoordinator" => Redirect("/BusCoordinator/Dashboard/Index"),
        "Parent" => Redirect("/Parent/Dashboard/Index"),
        "Student" => Redirect("/Student/Dashboard/Index"),
        _ => RedirectToAction(nameof(Login))
    };
}