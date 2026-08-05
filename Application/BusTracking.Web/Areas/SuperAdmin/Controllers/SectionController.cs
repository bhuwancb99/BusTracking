namespace BusTracking.Web.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin"), Authorize(Roles = "SuperAdmin")]
    public class SectionController : Controller
    {
        private readonly ISectionService _sectionService;
        private readonly IStandardService _standardService;

        public SectionController(ISectionService sectionService, IStandardService standardService)
        {
            _sectionService = sectionService;
            _standardService = standardService;
        }

        public async Task<IActionResult> Index(int? standardId)
        {
            var standards = (await _standardService.GetActiveStandardsAsync()).Data ?? new();
            int selectedStandardId = standardId ?? standards.FirstOrDefault()?.StandardId ?? 0;

            ViewBag.Standards = standards;
            ViewBag.SelectedStandardId = selectedStandardId;

            var sections = selectedStandardId > 0
                ? (await _sectionService.GetSectionsByStandardAsync(selectedStandardId)).Data ?? new()
                : new List<SectionDto>();

            return View(sections);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int standardId)
        {
            var standards = (await _standardService.GetActiveStandardsAsync()).Data ?? new();
            ViewBag.Standards = standards;

            return View(new CreateSectionDto { StandardId = standardId, SectionName = "B" });
        }

        // POST /SuperAdmin/Section/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateSectionDto dto)
        {
            if (!ModelState.IsValid)
            {
                var stdResult = await _standardService.GetByIdAsync(dto.StandardId);
                ViewBag.Standard = stdResult.Data;
                return View(dto);
            }

            var r = await _sectionService.CreateAsync(dto);
            if (!r.Success)
            {
                var stdResult = await _standardService.GetByIdAsync(dto.StandardId);
                ViewBag.Standard = stdResult.Data;
                ModelState.AddModelError("", r.Message);
                return View(dto);
            }

            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Index), new { standardId = dto.StandardId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var res = await _sectionService.GetByIdAsync(id);
            if (!res.Success || res.Data == null) return NotFound();

            var standards = (await _standardService.GetActiveStandardsAsync()).Data ?? new();
            ViewBag.Standards = standards;

            var dto = new UpdateSectionDto
            {
                SectionName = res.Data.SectionName,
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
            if (!ModelState.IsValid)
            {
                ViewBag.Standards = (await _standardService.GetActiveStandardsAsync()).Data ?? new();
                ViewBag.SectionId = id;
                ViewBag.StandardId = standardId;
                return View(dto);
            }

            var r = await _sectionService.UpdateAsync(id, dto);
            if (!r.Success)
            {
                ModelState.AddModelError("", r.Message);
                ViewBag.Standards = (await _standardService.GetActiveStandardsAsync()).Data ?? new();
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
            var r = await _sectionService.DeleteAsync(id);
            if (!r.Success)
            {
                TempData["ErrorMessage"] = r.Message;
            }
            else
            {
                TempData["SuccessMessage"] = r.Message;
            }
            return RedirectToAction(nameof(Index), new { standardId });
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(int id)
        {
            var r = await _sectionService.ToggleActiveAsync(id);
            return Json(r);
        }
    }
}
