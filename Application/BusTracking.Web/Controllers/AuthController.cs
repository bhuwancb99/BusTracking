using BusTracking.Common.DTOs.Auth;
using BusTracking.Common.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BusTracking.Web.Controllers;

public class AuthController : BaseController
{
    private readonly IAuthService _auth;
    public AuthController(IAuthService auth) => _auth = auth;

    // ── Login ────────────────────────────────────────────────────────
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToDashboard(User.FindFirstValue(ClaimTypes.Role) ?? "");

        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

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

        // Build cookie claims
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
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = result.Data.Expiry
            });

        // Return to requested URL if valid, else role-based area
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToDashboard(result.Data.Role);
    }

    // ── Logout ───────────────────────────────────────────────────────
    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    // ── Forgot Password ──────────────────────────────────────────────
    [HttpGet]
    public IActionResult ForgotPassword() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto model)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await _auth.ForgotPasswordAsync(model);
        TempData["Message"] = result.Message;
        return RedirectToAction(nameof(ForgotPasswordConfirmation));
    }

    [HttpGet]
    public IActionResult ForgotPasswordConfirmation() => View();

    // ── Reset Password ───────────────────────────────────────────────
    [HttpGet]
    public IActionResult ResetPassword(string token)
    {
        ViewBag.Token = token;
        return View(new ResetPasswordDto { Token = token });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await _auth.ResetPasswordAsync(model);
        if (!result.Success) { ModelState.AddModelError("", result.Message); return View(model); }
        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Login));
    }

    // ── Change Password ──────────────────────────────────────────────
    [Authorize, HttpGet]
    public IActionResult ChangePassword() => View();

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto model)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await _auth.ChangePasswordAsync(CurrentUserId, model);
        if (!result.Success) { ModelState.AddModelError("", result.Message); return View(model); }
        TempData["SuccessMessage"] = "Password changed successfully.";
        return RedirectToAction("Index", "Profile");
    }

    // ── Access Denied ────────────────────────────────────────────────
    public IActionResult AccessDenied() => View();

    // ── Role → Area redirect helper ──────────────────────────────────
    private IActionResult RedirectToDashboard(string role) => role switch
    {
        "SuperAdmin" => RedirectToAction("Index", "Dashboard", new { area = "SuperAdmin" }),
        "BusCoordinator" => RedirectToAction("Index", "Dashboard", new { area = "BusCoordinator" }),
        "Parent" => RedirectToAction("Index", "Dashboard", new { area = "Parent" }),
        "Student" => RedirectToAction("Index", "Dashboard", new { area = "Student" }),
        _ => RedirectToAction(nameof(Login))
    };
}
