namespace BusTracking.Web.Areas.BusCoordinator.Controllers
{
    [Area("BusCoordinator"), Authorize(Roles = "BusCoordinator")]
    public class StandardController : Controller
    {
        private readonly IStandardService _standard;
        public StandardController(IStandardService standard) => _standard = standard;

        // GET /BusCoordinator/Standard
        public async Task<IActionResult> Index([FromQuery] string? search = null, [FromQuery] bool? isActive = null, [FromQuery] int page = 1)
        {
            if (!PermissionHelper.Can(User, "standard.view")) return Forbid();
            ViewBag.Search = search;
            ViewBag.IsActive = isActive;
            var r = await _standard.GetAllAsync(search, isActive, page);
            return View(r.Data ?? new PagedResult<StandardDto>());
        }

        // GET /BusCoordinator/Standard/Create
        [HttpGet]
        public IActionResult Create()
        {
            if (!PermissionHelper.Can(User, "standard.add")) return Forbid();
            return View(new CreateStandardDto());
        }

        // POST /BusCoordinator/Standard/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateStandardDto dto)
        {
            if (!PermissionHelper.Can(User, "standard.add")) return Forbid();
            if (!ModelState.IsValid)
                return View(dto);
            var r = await _standard.CreateAsync(dto);
            if (!r.Success)
            {
                ModelState.AddModelError("", r.Message);
                return View(dto);
            }
            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        // GET /BusCoordinator/Standard/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!PermissionHelper.Can(User, "standard.edit")) return Forbid();
            var r = await _standard.GetByIdAsync(id);
            if (!r.Success)
                return NotFound();
            ViewBag.StandardId = id;
            return View(new UpdateStandardDto
            {
                StandardName = r.Data!.StandardName,
                IsActive = r.Data.IsActive
            });
        }

        // POST /BusCoordinator/Standard/Edit/{id}
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateStandardDto dto)
        {
            if (!PermissionHelper.Can(User, "standard.edit")) return Forbid();
            if (!ModelState.IsValid)
            {
                ViewBag.StandardId = id;
                return View(dto);
            }
            var r = await _standard.UpdateAsync(id, dto);
            if (!r.Success)
            {
                ModelState.AddModelError("", r.Message);
                ViewBag.StandardId = id;
                return View(dto);
            }
            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        // GET /BusCoordinator/Standard/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            if (!PermissionHelper.Can(User, "standard.view")) return Forbid();
            var r = await _standard.GetByIdAsync(id);
            if (!r.Success)
                return NotFound();
            return View(r.Data);
        }

        // POST /BusCoordinator/Standard/Delete/{id}
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!PermissionHelper.Can(User, "standard.delete")) return Forbid();
            var r = await _standard.DeleteAsync(id);
            TempData[r.Success ? "SuccessMessage" : "ErrorMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        // POST /BusCoordinator/Standard/Toggle/{id}
        [HttpPost]
        public async Task<IActionResult> Toggle(int id)
        {
            if (!PermissionHelper.Can(User, "standard.edit")) return Forbid();
            var r = await _standard.ToggleActiveAsync(id);
            return Json(new { r.Success, r.Message });
        }
    }
}
