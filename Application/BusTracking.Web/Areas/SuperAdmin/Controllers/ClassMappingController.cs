namespace BusTracking.Web.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin"), Authorize(Roles = "SuperAdmin")]
    public class ClassMappingController : Controller
    {
        private readonly IClassMappingService _mappingService;
        private readonly IStandardService _standardService;
        private readonly ISectionService _sectionService;
        private readonly ISubjectService _subjectService;
        private readonly ITeacherService _teacherService;
        private readonly IAcademicYearService _academicYearService;

        public ClassMappingController(
            IClassMappingService mappingService,
            IStandardService standardService,
            ISectionService sectionService,
            ISubjectService subjectService,
            ITeacherService teacherService,
            IAcademicYearService academicYearService)
        {
            _mappingService = mappingService;
            _standardService = standardService;
            _sectionService = sectionService;
            _subjectService = subjectService;
            _teacherService = teacherService;
            _academicYearService = academicYearService;
        }

        public async Task<IActionResult> Index(int? academicYearId, int? standardId, int? sectionId)
        {
            var years = await _academicYearService.GetAcademicYearsAsync(1);
            var activeYear = years.FirstOrDefault(y => y.IsCurrent) ?? years.FirstOrDefault();
            int selectedYearId = academicYearId ?? activeYear?.AcademicYearId ?? 0;

            var standards = (await _standardService.GetActiveStandardsAsync()).Data ?? new();
            int selectedStandardId = standardId ?? standards.FirstOrDefault()?.StandardId ?? 0;

            var sections = selectedStandardId > 0
                ? (await _sectionService.GetSectionsByStandardAsync(selectedStandardId)).Data ?? new()
                : new();

            ViewBag.AcademicYears = years;
            ViewBag.SelectedYearId = selectedYearId;
            ViewBag.Standards = standards;
            ViewBag.SelectedStandardId = selectedStandardId;
            ViewBag.Sections = sections;
            ViewBag.SelectedSectionId = sectionId;

            ViewBag.Subjects = (await _subjectService.GetActiveSubjectsAsync()).Data ?? new();
            var teachersPaged = await _teacherService.GetTeachersAsync(null, null, 1, 100);
            ViewBag.Teachers = teachersPaged.Items ?? new();

            var mappings = (selectedYearId > 0 && selectedStandardId > 0)
                ? (await _mappingService.GetClassMappingsAsync(selectedYearId, selectedStandardId, sectionId)).Data ?? new()
                : new List<ClassSubjectTeacherDto>();

            return View(mappings);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(AssignClassSubjectTeacherDto dto)
        {
            var r = await _mappingService.AssignSubjectTeacherAsync(dto);
            if (!r.Success) TempData["ErrorMessage"] = r.Message;
            else TempData["SuccessMessage"] = r.Message;

            return RedirectToAction(nameof(Index), new
            {
                academicYearId = dto.AcademicYearId,
                standardId = dto.StandardId,
                sectionId = dto.SectionId
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Unassign(int id, int academicYearId, int standardId, int? sectionId)
        {
            var r = await _mappingService.UnassignSubjectTeacherAsync(id);
            if (!r.Success) TempData["ErrorMessage"] = r.Message;
            else TempData["SuccessMessage"] = r.Message;

            return RedirectToAction(nameof(Index), new
            {
                academicYearId,
                standardId,
                sectionId
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, AssignClassSubjectTeacherDto dto)
        {
            var r = await _mappingService.UpdateSubjectTeacherAsync(id, dto);
            if (!r.Success) TempData["ErrorMessage"] = r.Message;
            else TempData["SuccessMessage"] = r.Message;

            return RedirectToAction(nameof(Index), new
            {
                academicYearId = dto.AcademicYearId,
                standardId = dto.StandardId,
                sectionId = dto.SectionId
            });
        }
    }
}
