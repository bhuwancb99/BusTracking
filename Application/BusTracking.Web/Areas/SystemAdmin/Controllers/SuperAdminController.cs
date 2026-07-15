namespace BusTracking.Web.Areas.SystemAdmin.Controllers
{
    [Area("SystemAdmin")]
    [Authorize(Roles = "SystemAdmin")]
    public class SuperAdminController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IPasswordService _pwd;
        private readonly IImageService _img;

        public SuperAdminController(AppDbContext db, IPasswordService pwd, IImageService img)
        {
            _db = db;
            _pwd = pwd;
            _img = img;
        }

        public async Task<IActionResult> Index(string search, int page = 1)
        {
            var query = _db.Users
                .IgnoreQueryFilters()
                .Include(u => u.School)
                .Where(u => u.RoleId == 1) // roleId = 1 is SuperAdmin
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(u => u.UserName.Contains(s) || u.FullName.Contains(s) || (u.Email != null && u.Email.Contains(s)));
            }

            int pageSize = 10;
            int totalItems = await query.CountAsync();
            var items = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.TotalItems = totalItems;

            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var schools = await _db.Schools.IgnoreQueryFilters().Where(s => s.IsActive).ToListAsync();
            ViewBag.Schools = new SelectList(schools, "SchoolId", "SchoolName");
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user, string password, IFormFile? profileImage)
        {
            ModelState.Remove("PasswordHash");
            ModelState.Remove("PasswordSalt");
            ModelState.Remove("Role");

            if (string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("PasswordHash", "Password is required.");
            }

            if (!user.SchoolId.HasValue)
            {
                ModelState.AddModelError("SchoolId", "Please assign a school.");
            }

            // Check unique Username
            var exists = await _db.Users.IgnoreQueryFilters().AnyAsync(u => u.UserName == user.UserName.Trim());
            if (exists)
            {
                ModelState.AddModelError("UserName", "This username already exists. Please choose another username.");
            }

            if (!ModelState.IsValid)
            {
                var schools = await _db.Schools.IgnoreQueryFilters().Where(s => s.IsActive).ToListAsync();
                ViewBag.Schools = new SelectList(schools, "SchoolId", "SchoolName");
                return View(user);
            }

            var (hash, salt) = _pwd.HashPassword(password);
            user.RoleId = 1; // 1 = SuperAdmin
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
            user.UserName = user.UserName.Trim();
            user.FullName = user.FullName.Trim();
            user.Email = user.Email?.Trim();
            user.IsActive = true;
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            if (profileImage != null && profileImage.Length > 0)
            {
                try
                {
                    var imgUrl = await _img.SaveProfileImageAsync(profileImage, user.UserId, "superadmin", null);
                    user.ProfileImageUrl = imgUrl;
                    await _db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Profile image upload failed: " + ex.Message);
                    var schools = await _db.Schools.IgnoreQueryFilters().Where(s => s.IsActive).ToListAsync();
                    ViewBag.Schools = new SelectList(schools, "SchoolId", "SchoolName");
                    return View(user);
                }
            }

            TempData["SuccessMessage"] = "Super Admin account created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.UserId == id && u.RoleId == 1);
            if (user == null) return NotFound();

            var schools = await _db.Schools.IgnoreQueryFilters().Where(s => s.IsActive).ToListAsync();
            ViewBag.Schools = new SelectList(schools, "SchoolId", "SchoolName", user.SchoolId);
            return View(user);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User model, string? password, IFormFile? profileImage)
        {
            ModelState.Remove("PasswordHash");
            ModelState.Remove("PasswordSalt");
            ModelState.Remove("Role");

            var user = await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.UserId == id && u.RoleId == 1);
            if (user == null) return NotFound();

            if (!model.SchoolId.HasValue)
            {
                ModelState.AddModelError("SchoolId", "Please assign a school.");
            }

            // Check unique Username
            var exists = await _db.Users.IgnoreQueryFilters()
                .AnyAsync(u => u.UserName == model.UserName.Trim() && u.UserId != id);
            if (exists)
            {
                ModelState.AddModelError("UserName", "This username already exists. Please choose another username.");
            }

            if (!ModelState.IsValid)
            {
                var schools = await _db.Schools.IgnoreQueryFilters().Where(s => s.IsActive).ToListAsync();
                ViewBag.Schools = new SelectList(schools, "SchoolId", "SchoolName", model.SchoolId);
                return View(model);
            }

            if (!string.IsNullOrWhiteSpace(password))
            {
                var (hash, salt) = _pwd.HashPassword(password);
                user.PasswordHash = hash;
                user.PasswordSalt = salt;
            }

            if (profileImage != null && profileImage.Length > 0)
            {
                try
                {
                    var imgUrl = await _img.SaveProfileImageAsync(profileImage, user.UserId, "superadmin", user.ProfileImageUrl);
                    user.ProfileImageUrl = imgUrl;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Profile image upload failed: " + ex.Message);
                    var schools = await _db.Schools.IgnoreQueryFilters().Where(s => s.IsActive).ToListAsync();
                    ViewBag.Schools = new SelectList(schools, "SchoolId", "SchoolName", model.SchoolId);
                    return View(model);
                }
            }

            user.UserName = model.UserName.Trim();
            user.FullName = model.FullName.Trim();
            user.Email = model.Email?.Trim();
            user.PhoneNumber = model.PhoneNumber?.Trim();
            user.SchoolId = model.SchoolId;
            user.IsActive = model.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Super Admin account updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int id, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword != confirmPassword)
            {
                TempData["ErrorMessage"] = "Passwords are empty or do not match.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.UserId == id && u.RoleId == 1);
            if (user == null) return NotFound();

            var (hash, salt) = _pwd.HashPassword(newPassword);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
            user.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Super Admin password reset successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var user = await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.UserId == id && u.RoleId == 1);
            if (user == null) return NotFound();

            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Json(new { success = true, isActive = user.IsActive });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> CheckUsername(string userName, int? excludeUserId = null)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return Json(new { success = false, message = "Username is required." });

            var query = _db.Users.IgnoreQueryFilters().Where(u => u.UserName == userName.Trim());
            if (excludeUserId.HasValue)
                query = query.Where(u => u.UserId != excludeUserId.Value);

            var exists = await query.AnyAsync();
            return Json(new { success = !exists, message = exists ? "This username already exists. Please choose another username." : "Username is available." });
        }
    }
}
