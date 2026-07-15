namespace BusTracking.Web.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin"), Authorize(Roles = "SuperAdmin")]
    public class StandardController : Controller
    {
        private readonly IStandardService _standard;
        public StandardController(IStandardService standard) => _standard = standard;

        // GET /SuperAdmin/Standard
        public async Task<IActionResult> Index([FromQuery] string? search = null, [FromQuery] bool? isActive = null, [FromQuery] int page = 1)
        {
            ViewBag.Search = search;
            ViewBag.IsActive = isActive;
            var r = await _standard.GetAllAsync(search, isActive, page);
            return View(r.Data ?? new PagedResult<StandardDto>());
        }

        // GET /SuperAdmin/Standard/Create
        [HttpGet] public IActionResult Create() => View(new CreateStandardDto());

        // POST /SuperAdmin/Standard/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateStandardDto dto)
        {
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

        // GET /SuperAdmin/Standard/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
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

        // POST /SuperAdmin/Standard/Edit/{id}
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateStandardDto dto)
        {
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

        // GET /SuperAdmin/Standard/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            var r = await _standard.GetByIdAsync(id);
            if (!r.Success)
                return NotFound();
            return View(r.Data);
        }

        // POST /SuperAdmin/Standard/Delete/{id}
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var r = await _standard.DeleteAsync(id);
            TempData[r.Success ? "SuccessMessage" : "ErrorMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        // POST /SuperAdmin/Standard/Toggle/{id}
        [HttpPost]
        public async Task<IActionResult> Toggle(int id)
        {
            var r = await _standard.ToggleActiveAsync(id);
            return Json(new { r.Success, r.Message });
        }
    }
}
