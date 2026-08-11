namespace BusTracking.Web.Areas.Teacher.Controllers
{
    [Area("Teacher"), Authorize(Roles = "Teacher")]
    public class AttendanceController : Controller
    {
        private readonly IAttendanceService _attendanceService;
        private readonly IStandardService _standardService;
        private readonly ISectionService _sectionService;
        private readonly IAcademicYearService _academicYearService;

        public AttendanceController(
            IAttendanceService attendanceService,
            IStandardService standardService,
            ISectionService sectionService,
            IAcademicYearService academicYearService)
        {
            _attendanceService = attendanceService;
            _standardService = standardService;
            _sectionService = sectionService;
            _academicYearService = academicYearService;
        }

        public async Task<IActionResult> Index(int? academicYearId, int? standardId, int? sectionId, DateTime? date)
        {
            var years = await _academicYearService.GetAcademicYearsAsync(1);
            var activeYear = years.FirstOrDefault(y => y.IsCurrent) ?? years.FirstOrDefault();
            int selectedYearId = academicYearId ?? activeYear?.AcademicYearId ?? 0;

            var standards = (await _standardService.GetActiveStandardsAsync()).Data ?? new();
            int selectedStandardId = standardId ?? standards.FirstOrDefault()?.StandardId ?? 0;

            var sections = selectedStandardId > 0
                ? (await _sectionService.GetSectionsByStandardAsync(selectedStandardId)).Data ?? new()
                : new();

            DateTime selectedDate = date?.Date ?? DateTime.Today;

            ViewBag.AcademicYears = years;
            ViewBag.SelectedYearId = selectedYearId;
            ViewBag.Standards = standards;
            ViewBag.SelectedStandardId = selectedStandardId;
            ViewBag.Sections = sections;
            ViewBag.SelectedSectionId = sectionId;
            ViewBag.SelectedDate = selectedDate;

            var studentsResult = (selectedYearId > 0 && selectedStandardId > 0)
                ? await _attendanceService.GetStudentsForAttendanceAsync(selectedYearId, selectedStandardId, sectionId, selectedDate)
                : null;

            ViewBag.Students = studentsResult?.Data ?? new List<StudentAttendanceRowDto>();

            var report = (selectedYearId > 0 && selectedStandardId > 0)
                ? (await _attendanceService.GetAttendanceReportAsync(selectedYearId, selectedStandardId, sectionId, selectedDate)).Data
                : new AttendanceSummaryReportDto();

            return View(report);
        }

        [HttpGet]
        public async Task<IActionResult> GetSectionsByStandard(int standardId)
        {
            var res = await _sectionService.GetSectionsByStandardAsync(standardId);
            return Json(res.Data ?? new List<SectionDto>());
        }

        [HttpPost]
        public async Task<IActionResult> SaveManual([FromBody] ManualAttendanceBatchDto dto)
        {
            var userId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : 1;
            var r = await _attendanceService.SaveManualAttendanceBatchAsync(dto, userId);
            return Json(r);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessFaceScan([FromBody] FaceAttendanceScanRequestDto dto)
        {
            var userId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : 1;
            var r = await _attendanceService.ProcessFaceScanAttendanceAsync(dto, userId);
            return Json(r);
        }
    }
}
