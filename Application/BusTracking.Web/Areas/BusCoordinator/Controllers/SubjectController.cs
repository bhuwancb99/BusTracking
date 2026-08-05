namespace BusTracking.Web.Areas.BusCoordinator.Controllers
{
    [Area("BusCoordinator"), Authorize(Roles = "BusCoordinator")]
    public class SubjectController : Controller
    {
        private readonly ISubjectService _subjectService;

        public SubjectController(ISubjectService subjectService)
        {
            _subjectService = subjectService;
        }

        public async Task<IActionResult> Index(string? search, bool? isActive, int page = 1)
        {
            if (!PermissionHelper.Can(User, "subject.view")) return Forbid();

            ViewBag.Search = search;
            ViewBag.IsActive = isActive;

            var result = await _subjectService.GetAllAsync(search, isActive, page);
            return View(result.Data ?? new PagedResult<SubjectDto>());
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!PermissionHelper.Can(User, "subject.add")) return Forbid();
            return View(new CreateSubjectDto());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateSubjectDto dto)
        {
            if (!PermissionHelper.Can(User, "subject.add")) return Forbid();

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

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!PermissionHelper.Can(User, "subject.edit")) return Forbid();

            var r = await _subjectService.GetByIdAsync(id);
            if (!r.Success || r.Data == null) return NotFound();

            return View(new UpdateSubjectDto
            {
                SubjectName = r.Data.SubjectName,
                SubjectCode = r.Data.SubjectCode,
                IsActive = r.Data.IsActive
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateSubjectDto dto)
        {
            if (!PermissionHelper.Can(User, "subject.edit")) return Forbid();

            if (!ModelState.IsValid) return View(dto);

            var r = await _subjectService.UpdateAsync(id, dto);
            if (!r.Success)
            {
                ModelState.AddModelError("", r.Message);
                return View(dto);
            }

            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!PermissionHelper.Can(User, "subject.delete")) return Forbid();

            var r = await _subjectService.DeleteAsync(id);
            if (!r.Success) TempData["ErrorMessage"] = r.Message;
            else TempData["SuccessMessage"] = r.Message;

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(int id)
        {
            if (!PermissionHelper.Can(User, "subject.edit")) return Forbid();
            var r = await _subjectService.ToggleActiveAsync(id);
            return Json(r);
        }
    }
}
