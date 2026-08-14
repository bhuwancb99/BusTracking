namespace BusTracking.Web.Areas.Teacher.Controllers
{
    [Area("Teacher"), Authorize(Roles = "Teacher")]
    public class ExamController : Controller
    {
        private readonly IExamService _examService;
        private readonly IStandardService _standardService;
        private readonly ISectionService _sectionService;
        private readonly IAcademicYearService _academicYearService;

        private int CurrentUserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 1;

        public ExamController(
            IExamService examService,
            IStandardService standardService,
            ISectionService sectionService,
            IAcademicYearService academicYearService)
        {
            _examService = examService;
            _standardService = standardService;
            _sectionService = sectionService;
            _academicYearService = academicYearService;
        }

        public async Task<IActionResult> Index(int? examTermId, int? standardId, int? examScheduleId, int? sectionId)
        {
            var years = await _academicYearService.GetAcademicYearsAsync(1);
            var activeYear = years.Find(y => y.IsCurrent) ?? years.Find(y => y.IsActive);

            var termsRes = await _examService.GetExamTermsAsync(activeYear?.AcademicYearId);
            var terms = termsRes.Data ?? new();
            var stdsRes = await _standardService.GetActiveStandardsAsync();
            var stds = stdsRes.Data ?? new();

            ViewBag.ExamTerms = terms;
            ViewBag.SelectedTermId = examTermId;
            ViewBag.Standards = stds;
            ViewBag.SelectedStandardId = standardId;

            var schedulesRes = (examTermId.HasValue || standardId.HasValue)
                ? await _examService.GetExamSchedulesAsync(examTermId, standardId)
                : null;
            var schedules = schedulesRes?.Data ?? new List<ExamScheduleDto>();
            ViewBag.ExamSchedules = schedules;
            ViewBag.SelectedScheduleId = examScheduleId;

            var sectionsRes = standardId.HasValue && standardId.Value > 0
                ? await _sectionService.GetSectionsByStandardAsync(standardId.Value)
                : null;
            ViewBag.Sections = sectionsRes?.Data ?? new List<SectionDto>();
            ViewBag.SelectedSectionId = sectionId;

            var studentGrid = new List<StudentMarksGridItemDto>();
            if (examScheduleId.HasValue && examScheduleId.Value > 0 && sectionId.HasValue && sectionId.Value > 0)
            {
                var gridRes = await _examService.GetStudentMarksGridAsync(examScheduleId.Value, sectionId.Value);
                studentGrid = gridRes.Data ?? new List<StudentMarksGridItemDto>();
            }


            return View(studentGrid);
        }

        [HttpGet]
        public async Task<IActionResult> GetSectionsByStandard(int standardId)
        {
            var res = await _sectionService.GetSectionsByStandardAsync(standardId);
            return Json(res.Data ?? new());
        }

        [HttpGet]
        public async Task<IActionResult> GetSchedulesByTermAndStandard(int? examTermId, int? standardId)
        {
            var res = await _examService.GetExamSchedulesAsync(examTermId, standardId);
            return Json(res.Data ?? new());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveMarks([FromBody] SaveStudentMarksGridDto dto)
        {
            if (dto == null || dto.ExamScheduleId <= 0)
            {
                return Json(new { success = false, message = "Invalid schedule selection." });
            }

            var res = await _examService.SaveStudentMarksGridAsync(dto, CurrentUserId);
            return Json(new { success = res.Success, message = res.Message });
        }

        public async Task<IActionResult> Datesheet(int? examTermId, int? standardId)
        {
            var years = await _academicYearService.GetAcademicYearsAsync(1);
            var activeYear = years.Find(y => y.IsCurrent) ?? years.Find(y => y.IsActive);

            var termsRes = await _examService.GetExamTermsAsync(activeYear?.AcademicYearId);
            var terms = termsRes.Data ?? new();
            var stds = await _standardService.GetActiveStandardsAsync();

            ViewBag.ExamTerms = terms;
            ViewBag.SelectedTermId = examTermId;
            ViewBag.Standards = stds.Data ?? new();
            ViewBag.SelectedStandardId = standardId;

            var schedulesRes = await _examService.GetExamSchedulesAsync(examTermId, standardId);
            return View(schedulesRes.Data ?? new());
        }
    }
}
