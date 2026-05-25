namespace BusTracking.Web.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin"), Authorize(Roles = "SuperAdmin")]
    public class AppConfigController : Controller
    {
        private readonly IAppConfigService _config;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        public AppConfigController(IAppConfigService config) => _config = config;

        // GET /SuperAdmin/AppConfig
        public async Task<IActionResult> Index([FromQuery] string? platform, [FromQuery] string? search, [FromQuery] bool? isActive)
        {
            ViewBag.Platform = platform;
            ViewBag.Search = search;
            ViewBag.IsActive = isActive;
            var r = await _config.GetAllAsync(platform, search, isActive);
            return View(r.Data ?? []);
        }

        // GET /SuperAdmin/AppConfig/Create
        [HttpGet] public IActionResult Create() => View(new CreateAppConfigDto());

        // POST /SuperAdmin/AppConfig/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAppConfigDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);
            var r = await _config.CreateAsync(dto, UserId);
            if (!r.Success)
            {
                ModelState.AddModelError("", r.Message); return View(dto);
            }
            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        // GET /SuperAdmin/AppConfig/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var r = await _config.GetByIdAsync(id);
            if (!r.Success)
                return NotFound();
            ViewBag.ConfigId = id;
            return View(new UpdateAppConfigDto
            {
                ConfigKey = r.Data!.ConfigKey,
                ConfigValue = r.Data.ConfigValue,
                Description = r.Data.Description,
                Platform = Enum.Parse<ConfigPlatform>(r.Data.Platform),
                IsActive = r.Data.IsActive
            });
        }

        // POST /SuperAdmin/AppConfig/Edit/{id}
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateAppConfigDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ConfigId = id;
                return View(dto);
            }
            var r = await _config.UpdateAsync(id, dto);
            if (!r.Success)
            {
                ModelState.AddModelError("", r.Message);
                ViewBag.ConfigId = id;
                return View(dto);
            }
            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        // GET /SuperAdmin/AppConfig/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            var r = await _config.GetByIdAsync(id);
            if (!r.Success)
                return NotFound();
            return View(r.Data);
        }

        // POST /SuperAdmin/AppConfig/Delete/{id}
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var r = await _config.DeleteAsync(id);
            TempData[r.Success ? "SuccessMessage" : "ErrorMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        // POST /SuperAdmin/AppConfig/Toggle/{id}
        [HttpPost]
        public async Task<IActionResult> Toggle(int id)
        {
            var r = await _config.ToggleActiveAsync(id);
            return Json(new { r.Success, r.Message });
        }
    }
}
