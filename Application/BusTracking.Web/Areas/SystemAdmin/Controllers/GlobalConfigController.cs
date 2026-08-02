namespace BusTracking.Web.Areas.SystemAdmin.Controllers
{
    [Area("SystemAdmin")]
    [Authorize(Roles = "SystemAdmin")]
    public class GlobalConfigController : Controller
    {
        private readonly IGlobalConfigService _globalConfigService;

        public GlobalConfigController(IGlobalConfigService globalConfigService)
        {
            _globalConfigService = globalConfigService;
        }

        public async Task<IActionResult> Index(string search, bool? isActive, int page = 1)
        {
            var result = await _globalConfigService.GetAllAsync(search, isActive, page, 10);

            ViewBag.Search = search;
            ViewBag.IsActive = isActive;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.TotalItems = result.TotalCount;

            return View(result.Items);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateGlobalConfigDto { IsActive = true });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateGlobalConfigDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _globalConfigService.CreateAsync(model);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View(model);
            }

            TempData["SuccessMessage"] = "Global configuration created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _globalConfigService.GetByIdAsync(id);
            if (!result.Success || result.Data == null) return NotFound();

            var updateDto = new UpdateGlobalConfigDto
            {
                GlobalConfigValue = result.Data.GlobalConfigValue,
                Description = result.Data.Description,
                IsActive = result.Data.IsActive
            };

            ViewBag.GlobalConfigId = id;
            ViewBag.GlobalConfigKey = result.Data.GlobalConfigKey;

            return View(updateDto);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateGlobalConfigDto model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.GlobalConfigId = id;
                return View(model);
            }

            var result = await _globalConfigService.UpdateAsync(id, model);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                ViewBag.GlobalConfigId = id;
                return View(model);
            }

            TempData["SuccessMessage"] = "Global configuration updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int id)
        {
            var result = await _globalConfigService.ToggleActiveAsync(id);
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
