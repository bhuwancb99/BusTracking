using BusTracking.Web.Helpers;

namespace BusTracking.Web.Areas.BusCoordinator.Controllers
{
    [Area("BusCoordinator"), Authorize(Roles = "BusCoordinator")]
    public class SubAdminController : Controller
    {
        private readonly ISubAdminService _sa;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        public SubAdminController(ISubAdminService sa) => _sa = sa;

        // GET /BusCoordinator/SubAdmin
        public async Task<IActionResult> Index(int page = 1, string? search = null, string? status = "Active")
        {
            if (!PermissionHelper.Can(User, "subadmin.view")) return Forbid();
            var normalised = (status == "Both" || string.IsNullOrEmpty(status)) ? null : status;
            ViewBag.Status = status;
            ViewBag.CurrentUserId = UserId;   // view uses this to hide edit/delete/toggle for own row
            var result = await _sa.GetAllAsync(page, search, normalised);
            return View(result);
        }

        // GET /BusCoordinator/SubAdmin/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            if (!PermissionHelper.Can(User, "subadmin.view")) return Forbid();
            // Own record is viewable — edit/delete buttons are hidden in the view
            var r = await _sa.GetByIdAsync(id);
            return r.Success ? View(r.Data) : NotFound();
        }

        // GET /BusCoordinator/SubAdmin/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!PermissionHelper.Can(User, "subadmin.add")) return Forbid();
            ViewBag.AllPermissions = await _sa.GetAllPermissionsAsync();
            return View(new CreateSubAdminDto());
        }

        // POST /BusCoordinator/SubAdmin/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateSubAdminDto m)
        {
            if (!PermissionHelper.Can(User, "subadmin.add")) return Forbid();
            if (!ModelState.IsValid)
            {
                ViewBag.AllPermissions = await _sa.GetAllPermissionsAsync();
                return View(m);
            }
            var r = await _sa.CreateAsync(m, UserId);
            if (!r.Success)
            {
                ModelState.AddModelError("", r.Message);
                ViewBag.AllPermissions = await _sa.GetAllPermissionsAsync();
                return View(m);
            }
            TempData["CreatedUser"] = System.Text.Json.JsonSerializer.Serialize(r.Data);
            TempData["SuccessMessage"] = "Coordinator created.";
            return RedirectToAction(nameof(Index));
        }

        // GET /BusCoordinator/SubAdmin/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!PermissionHelper.Can(User, "subadmin.edit")) return Forbid();
            // Block editing own record via URL
            if (id == UserId) return Forbid();
            var r = await _sa.GetByIdAsync(id);
            if (!r.Success) return NotFound();
            ViewBag.SubAdminId = id;
            ViewBag.AllPermissions = await _sa.GetAllPermissionsAsync();
            var permIds = await _sa.GetPermissionIdsAsync(id);
            return View(new UpdateSubAdminDto
            {
                FullName      = r.Data!.FullName,
                UserName    = r.Data!.UserName,
                Email       = r.Data.Email,
                PhoneNumber   = r.Data.PhoneNumber,
                IsActive      = r.Data.IsActive,
                PermissionIds = permIds
            });
        }

        // POST /BusCoordinator/SubAdmin/Edit/{id}
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateSubAdminDto m)
        {
            if (!PermissionHelper.Can(User, "subadmin.edit")) return Forbid();
            if (id == UserId) return Forbid();
            if (!ModelState.IsValid)
            {
                ViewBag.SubAdminId = id;
                ViewBag.AllPermissions = await _sa.GetAllPermissionsAsync();
                return View(m);
            }
            var r = await _sa.UpdateAsync(id, m);
            if (!r.Success)
            {
                ModelState.AddModelError("", r.Message);
                ViewBag.SubAdminId = id;
                ViewBag.AllPermissions = await _sa.GetAllPermissionsAsync();
                return View(m);
            }
            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        // POST /BusCoordinator/SubAdmin/Delete/{id}
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!PermissionHelper.Can(User, "subadmin.delete")) return Forbid();
            if (id == UserId) return Forbid();
            await _sa.DeleteAsync(id);
            TempData["SuccessMessage"] = "Marked inactive.";
            return RedirectToAction(nameof(Index));
        }

        // POST /BusCoordinator/SubAdmin/Toggle (AJAX)
        [HttpPost]
        public async Task<IActionResult> Toggle(int id)
        {
            if (!PermissionHelper.Can(User, "subadmin.edit")) return Forbid();
            if (id == UserId) return Json(new { Success = false, Message = "You cannot toggle your own account." });
            var r = await _sa.ToggleActiveAsync(id);
            return Json(new { r.Success, r.Message });
        }

        // POST /BusCoordinator/SubAdmin/ResetPassword (AJAX)
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int id)
        {
            if (!PermissionHelper.Can(User, "subadmin.edit")) return Forbid();
            if (id == UserId) return Json(new { Success = false, Message = "You cannot reset your own password here." });
            var r = await _sa.ResetPasswordAsync(id);
            return Json(new
            {
                r.Success,
                r.Message,
                password = r.Data?.PlainPassword,
                fullName = r.Data?.FullName,
                email    = r.Data?.Email
            });
        }
    }
}
