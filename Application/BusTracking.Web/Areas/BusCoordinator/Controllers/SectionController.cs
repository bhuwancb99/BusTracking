namespace BusTracking.Web.Areas.BusCoordinator.Controllers
{
    [Area("BusCoordinator"), Authorize(Roles = "BusCoordinator")]
    public class SectionController : Controller
    {
        private readonly ISectionService _sectionService;
        private readonly IStandardService _standardService;
        private readonly ITeacherService _teacherService;

        public SectionController(ISectionService sectionService, IStandardService standardService, ITeacherService teacherService)
        {
            _sectionService = sectionService;
            _standardService = standardService;
            _teacherService = teacherService;
        }

        public async Task<IActionResult> Index(int? standardId)
        {
            if (!PermissionHelper.Can(User, "section.view")) return Forbid();

            var standards = (await _standardService.GetActiveStandardsAsync()).Data ?? new();
            int selectedStandardId = standardId ?? standards.FirstOrDefault()?.StandardId ?? 0;

            ViewBag.Standards = standards;
            ViewBag.SelectedStandardId = selectedStandardId;

            var sections = selectedStandardId > 0
                ? (await _sectionService.GetSectionsByStandardAsync(selectedStandardId)).Data ?? new()
                : new();

            return View(sections);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int standardId)
        {
            if (!PermissionHelper.Can(User, "section.add")) return Forbid();

            var standards = (await _standardService.GetActiveStandardsAsync()).Data ?? new();
            ViewBag.Standards = standards;

            var teachersPaged = await _teacherService.GetTeachersAsync(null, null, 1, 200);
            ViewBag.Teachers = teachersPaged.Items ?? new();

            return View(new CreateSectionDto { StandardId = standardId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateSectionDto dto)
        {
            if (!PermissionHelper.Can(User, "section.add")) return Forbid();

            if (!ModelState.IsValid)
            {
                ViewBag.Standards = (await _standardService.GetActiveStandardsAsync()).Data ?? new();
                ViewBag.Teachers = (await _teacherService.GetTeachersAsync(null, null, 1, 200)).Items ?? new();
                return View(dto);
            }

            var r = await _sectionService.CreateAsync(dto);
            if (!r.Success)
            {
                ModelState.AddModelError("", r.Message);
                ViewBag.Standards = (await _standardService.GetActiveStandardsAsync()).Data ?? new();
                ViewBag.Teachers = (await _teacherService.GetTeachersAsync(null, null, 1, 200)).Items ?? new();
                return View(dto);
            }

            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Index), new { standardId = dto.StandardId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!PermissionHelper.Can(User, "section.edit")) return Forbid();

            var res = await _sectionService.GetByIdAsync(id);
            if (!res.Success || res.Data == null) return NotFound();

            var standards = (await _standardService.GetActiveStandardsAsync()).Data ?? new();
            ViewBag.Standards = standards;

            var teachersPaged = await _teacherService.GetTeachersAsync(null, null, 1, 200);
            ViewBag.Teachers = teachersPaged.Items ?? new();

            var dto = new UpdateSectionDto
            {
                SectionId = res.Data.SectionId,
                SectionName = res.Data.SectionName,
                ClassTeacherId = res.Data.ClassTeacherId,
                IsActive = res.Data.IsActive
            };
            ViewBag.SectionId = id;
            ViewBag.StandardId = res.Data.StandardId;
            ViewBag.StandardName = res.Data.StandardName;

            return View(dto);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateSectionDto dto, int standardId)
        {
            if (!PermissionHelper.Can(User, "section.edit")) return Forbid();

            if (!ModelState.IsValid)
            {
                ViewBag.Standards = (await _standardService.GetActiveStandardsAsync()).Data ?? new();
                ViewBag.Teachers = (await _teacherService.GetTeachersAsync(null, null, 1, 200)).Items ?? new();
                ViewBag.SectionId = id;
                ViewBag.StandardId = standardId;
                return View(dto);
            }

            var r = await _sectionService.UpdateAsync(id, dto);
            if (!r.Success)
            {
                ModelState.AddModelError("", r.Message);
                ViewBag.Standards = (await _standardService.GetActiveStandardsAsync()).Data ?? new();
                ViewBag.Teachers = (await _teacherService.GetTeachersAsync(null, null, 1, 200)).Items ?? new();
                ViewBag.SectionId = id;
                ViewBag.StandardId = standardId;
                return View(dto);
            }

            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Index), new { standardId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int standardId)
        {
            if (!PermissionHelper.Can(User, "section.delete")) return Forbid();

            var r = await _sectionService.DeleteAsync(id);
            if (!r.Success) TempData["ErrorMessage"] = r.Message;
            else TempData["SuccessMessage"] = r.Message;

            return RedirectToAction(nameof(Index), new { standardId });
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(int id)
        {
            if (!PermissionHelper.Can(User, "section.edit")) return Forbid();
            var r = await _sectionService.ToggleActiveAsync(id);
            return Json(r);
        }
    }
}
