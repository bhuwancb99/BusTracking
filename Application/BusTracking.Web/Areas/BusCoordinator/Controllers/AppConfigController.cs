using BusTracking.Web.Helpers;

namespace BusTracking.Web.Areas.BusCoordinator.Controllers
{
    [Area("BusCoordinator"), Authorize(Roles = "BusCoordinator")]
    public class AppConfigController : Controller
    {
        private readonly IAppConfigService _config;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        public AppConfigController(IAppConfigService config) => _config = config;

        // GET /BusCoordinator/AppConfig
        public async Task<IActionResult> Index(string? platform, string? search, bool? isActive)
        {
            if (!PermissionHelper.Can(User, "appconfig.view")) return Forbid();
            ViewBag.Platform = platform;
            ViewBag.Search   = search;
            ViewBag.IsActive = isActive;
            var r = await _config.GetAllAsync(platform, search, isActive);
            return View(r.Data ?? []);
        }

        // GET /BusCoordinator/AppConfig/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            if (!PermissionHelper.Can(User, "appconfig.view")) return Forbid();
            var r = await _config.GetByIdAsync(id);
            return r.Success ? View(r.Data) : NotFound();
        }

        // GET /BusCoordinator/AppConfig/Create
        [HttpGet]
        public IActionResult Create()
        {
            if (!PermissionHelper.Can(User, "appconfig.add")) return Forbid();
            return View(new CreateAppConfigDto());
        }

        // POST /BusCoordinator/AppConfig/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAppConfigDto dto)
        {
            if (!PermissionHelper.Can(User, "appconfig.add")) return Forbid();
            if (!ModelState.IsValid) return View(dto);
            var r = await _config.CreateAsync(dto, UserId);
            if (!r.Success) { ModelState.AddModelError("", r.Message); return View(dto); }
            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        // GET /BusCoordinator/AppConfig/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!PermissionHelper.Can(User, "appconfig.edit")) return Forbid();
            var r = await _config.GetByIdAsync(id);
            if (!r.Success) return NotFound();
            ViewBag.ConfigId = id;
            return View(new UpdateAppConfigDto
            {
                ConfigKey   = r.Data!.ConfigKey,
                ConfigValue = r.Data.ConfigValue,
                Description = r.Data.Description,
                Platform    = r.Data.Platform,
                IsActive    = r.Data.IsActive
            });
        }

        // POST /BusCoordinator/AppConfig/Edit/{id}
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateAppConfigDto dto)
        {
            if (!PermissionHelper.Can(User, "appconfig.edit")) return Forbid();
            if (!ModelState.IsValid) { ViewBag.ConfigId = id; return View(dto); }
            var r = await _config.UpdateAsync(id, dto);
            if (!r.Success) { ModelState.AddModelError("", r.Message); ViewBag.ConfigId = id; return View(dto); }
            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        // POST /BusCoordinator/AppConfig/Delete/{id}
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!PermissionHelper.Can(User, "appconfig.delete")) return Forbid();
            var r = await _config.DeleteAsync(id);
            TempData[r.Success ? "SuccessMessage" : "ErrorMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        // POST /BusCoordinator/AppConfig/Toggle/{id}  (AJAX)
        [HttpPost]
        public async Task<IActionResult> Toggle(int id)
        {
            if (!PermissionHelper.Can(User, "appconfig.edit")) return Forbid();
            var r = await _config.ToggleActiveAsync(id);
            return Json(new { r.Success, r.Message });
        }
    }
}
