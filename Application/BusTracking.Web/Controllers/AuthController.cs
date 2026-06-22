namespace BusTracking.Web.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _auth;
    public AuthController(IAuthService auth) => _auth = auth;

    private int CurrentUserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    [HttpGet]
    public IActionResult Login()
    {
        // Already logged in → go straight to their area dashboard.
        // Do NOT read ReturnUrl here — that is what causes the loop.
        if (User.Identity?.IsAuthenticated == true)
            return DashboardRedirect(User.FindFirstValue(ClaimTypes.Role));
        return View(new LoginDto());
    }

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
            new(ClaimTypes.Email,          result.Data.Email ?? result.Data.UserName),
            new("username",                result.Data.UserName),
            new(ClaimTypes.Name,           result.Data.FullName),
            new(ClaimTypes.Role,           result.Data.Role),
        };

        // For BusCoordinators, load their assigned permissions and add as claims
        if (result.Data.Role == "BusCoordinator")
        {
            var permKeys = await _auth.GetCoordinatorPermissionsAsync(result.Data.UserId);
            foreach (var key in permKeys)
                claims.Add(new Claim("permission", key));
        }

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

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet] public IActionResult ForgotPassword() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto model)
    {
        if (!ModelState.IsValid)
            return View(model);
        var r = await _auth.ForgotPasswordAsync(model);
        TempData["Message"] = r.Message;
        return RedirectToAction(nameof(ForgotPasswordConfirmation));
    }

    [HttpGet] public IActionResult ForgotPasswordConfirmation() => View();

    [HttpGet]
    public IActionResult ResetPassword(string token) => View(new ResetPasswordDto { Token = token });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
    {
        if (!ModelState.IsValid)
            return View(model);
        var r = await _auth.ResetPasswordAsync(model);
        if (!r.Success)
        {
            ModelState.AddModelError("", r.Message);
            return View(model);
        }
        TempData["SuccessMessage"] = r.Message;
        return RedirectToAction(nameof(Login));
    }

    [Authorize, HttpGet]
    public IActionResult ChangePassword() => View();

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto model)
    {
        if (!ModelState.IsValid)
            return View(model);
        var r = await _auth.ChangePasswordAsync(CurrentUserId, model);
        if (!r.Success)
        {
            ModelState.AddModelError("", r.Message);
            return View(model);
        }
        TempData["SuccessMessage"] = "Password changed.";
        return RedirectToAction("Index", "Profile");
    }

    [HttpGet]
    public IActionResult AccessDenied(string? returnUrl = null)
    {
        // Coordinators get a richer, in-layout denied page inside their own area
        if (User.IsInRole("BusCoordinator"))
            return Redirect($"/BusCoordinator/AccessDenied/Index?returnUrl={Uri.EscapeDataString(returnUrl ?? "")}");
        return View();
    }

    // ── Role → dashboard redirect ─────────────────────────────────────────
    private IActionResult DashboardRedirect(string? role) => role switch
    {
        "SuperAdmin" => Redirect("/SuperAdmin/Dashboard/Index"),
        "BusCoordinator" => Redirect("/BusCoordinator/Dashboard/Index"),
        "Driver" => Redirect("/Driver/Dashboard/Index"),   // ← ADDED
        "Parent" => Redirect("/Parent/Dashboard/Index"),
        "Student" => Redirect("/Student/Dashboard/Index"),
        _ => RedirectToAction(nameof(Login))
    };
}