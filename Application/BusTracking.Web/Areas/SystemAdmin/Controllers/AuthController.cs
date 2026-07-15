namespace BusTracking.Web.Areas.SystemAdmin.Controllers
{
    [Area("SystemAdmin")]
    public class AuthController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IPasswordService _pwd;

        public AuthController(AppDbContext db, IPasswordService pwd)
        {
            _db = db;
            _pwd = pwd;
        }

        private int CurrentAdminId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("SystemAdmin"))
                return RedirectToAction("Index", "Dashboard", new { area = "SystemAdmin" });
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Username and password are required.");
                return View();
            }

            var admin = await _db.SystemAdministrators
                .FirstOrDefaultAsync(a => a.UserName == username.Trim());

            if (admin == null || !_pwd.VerifyPassword(password, admin.PasswordHash, admin.PasswordSalt))
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View();
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, admin.AdminId.ToString()),
                new("username", admin.UserName),
                new(ClaimTypes.Name, admin.FullName),
                new(ClaimTypes.Email, admin.Email ?? ""),
                new(ClaimTypes.Role, "SystemAdmin")
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)),
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });

            return RedirectToAction("Index", "Dashboard", new { area = "SystemAdmin" });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        [Authorize(Roles = "SystemAdmin"), HttpGet]
        public async Task<IActionResult> Profile()
        {
            var admin = await _db.SystemAdministrators.FindAsync(CurrentAdminId);
            if (admin == null) return NotFound();
            return View(admin);
        }

        [Authorize(Roles = "SystemAdmin"), HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(string fullName, string email, string userName)
        {
            var admin = await _db.SystemAdministrators.FindAsync(CurrentAdminId);
            if (admin == null) return NotFound();

            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(userName))
            {
                ModelState.AddModelError("", "Full Name and Username are required.");
                return View(admin);
            }

            // Check username uniqueness
            var exists = await _db.SystemAdministrators
                .AnyAsync(a => a.UserName == userName.Trim() && a.AdminId != admin.AdminId);
            if (exists)
            {
                ModelState.AddModelError("", "Username already exists.");
                return View(admin);
            }

            admin.FullName = fullName.Trim();
            admin.Email = email?.Trim();
            admin.UserName = userName.Trim();
            admin.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Profile updated successfully.";

            // Sign in again to refresh cookie claims
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, admin.AdminId.ToString()),
                new("username", admin.UserName),
                new(ClaimTypes.Name, admin.FullName),
                new(ClaimTypes.Email, admin.Email ?? ""),
                new(ClaimTypes.Role, "SystemAdmin")
            };
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));

            return RedirectToAction(nameof(Profile));
        }

        [Authorize(Roles = "SystemAdmin"), HttpGet]
        public IActionResult ChangePassword() => View();

        [Authorize(Roles = "SystemAdmin"), HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
            {
                ModelState.AddModelError("", "All fields are required.");
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "New passwords do not match.");
                return View();
            }

            var admin = await _db.SystemAdministrators.FindAsync(CurrentAdminId);
            if (admin == null) return NotFound();

            if (!_pwd.VerifyPassword(currentPassword, admin.PasswordHash, admin.PasswordSalt))
            {
                ModelState.AddModelError("", "Current password is incorrect.");
                return View();
            }

            var (hash, salt) = _pwd.HashPassword(newPassword);
            admin.PasswordHash = hash;
            admin.PasswordSalt = salt;
            admin.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Password changed successfully.";
            return RedirectToAction(nameof(Profile));
        }
    }
}
