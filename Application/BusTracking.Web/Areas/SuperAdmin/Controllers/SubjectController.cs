namespace BusTracking.Web.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin"), Authorize(Roles = "SuperAdmin")]
    public class SubjectController : Controller
    {
        private readonly ISubjectService _subjectService;

        public SubjectController(ISubjectService subjectService)
        {
            _subjectService = subjectService;
        }

        // GET /SuperAdmin/Subject
        public async Task<IActionResult> Index([FromQuery] string? search = null, [FromQuery] bool? isActive = null, [FromQuery] int page = 1)
        {
            ViewBag.Search = search;
            ViewBag.IsActive = isActive;
            var r = await _subjectService.GetAllAsync(search, isActive, page);
            return View(r.Data ?? new PagedResult<SubjectDto>());
        }

        // GET /SuperAdmin/Subject/Create
        [HttpGet]
        public IActionResult Create() => View(new CreateSubjectDto());

        // POST /SuperAdmin/Subject/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateSubjectDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            var r = await _subjectService.CreateAsync(dto);
            if (!r.Success)
            {
                ModelState.AddModelError("", r.Message);
                return View(dto);
            }

            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        // GET /SuperAdmin/Subject/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var r = await _subjectService.GetByIdAsync(id);
            if (!r.Success) return NotFound();
            ViewBag.SubjectId = id;
            return View(new UpdateSubjectDto
            {
                SubjectId = r.Data!.SubjectId,
                SubjectName = r.Data.SubjectName,
                SubjectCode = r.Data.SubjectCode,
                IsActive = r.Data.IsActive
            });
        }

        // POST /SuperAdmin/Subject/Edit/{id}
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateSubjectDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.SubjectId = id;
                return View(dto);
            }

            var r = await _subjectService.UpdateAsync(id, dto);
            if (!r.Success)
            {
                ViewBag.SubjectId = id;
                ModelState.AddModelError("", r.Message);
                return View(dto);
            }

            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var r = await _subjectService.DeleteAsync(id);
            if (!r.Success) TempData["ErrorMessage"] = r.Message;
            else TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(int id)
        {
            var r = await _subjectService.ToggleActiveAsync(id);
            return Json(r);
        }
    }
}
