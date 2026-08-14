namespace BusTracking.Web.Areas.Student.Controllers
{
    [Area("Student"), Authorize(Roles = "Student")]
    public class ExamController : Controller
    {
        private readonly IExamService _examService;
        private readonly IAcademicYearService _academicYearService;
        private readonly AppDbContext _db;

        private int CurrentUserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 1;

        public ExamController(
            IExamService examService,
            IAcademicYearService academicYearService,
            AppDbContext db)
        {
            _examService = examService;
            _academicYearService = academicYearService;
            _db = db;
        }

        public async Task<IActionResult> Index(int? examTermId)
        {
            var student = await _db.Students.FirstOrDefaultAsync(s => s.UserId == CurrentUserId);
            if (student == null) return View("Error");

            var years = await _academicYearService.GetAcademicYearsAsync(1);
            var activeYear = years.Find(y => y.IsCurrent) ?? years.Find(y => y.IsActive);

            var termsRes = await _examService.GetExamTermsAsync(activeYear?.AcademicYearId);
            var terms = termsRes.Data ?? new();
            var selectedTermId = examTermId ?? terms.Find(t => t.IsActive)?.ExamTermId ?? 0;

            ViewBag.ExamTerms = terms;
            ViewBag.SelectedTermId = selectedTermId;

            var schedulesRes = await _examService.GetExamSchedulesAsync(selectedTermId, student.StandardId);
            ViewBag.ExamSchedules = schedulesRes.Data ?? new();

            if (selectedTermId > 0)
            {
                var rcRes = await _examService.GetStudentReportCardAsync(student.StudentId, selectedTermId);
                ViewBag.ReportCard = rcRes.Data;
            }

            return View();
        }

        public async Task<IActionResult> ReportCard(int examTermId)
        {
            var student = await _db.Students.FirstOrDefaultAsync(s => s.UserId == CurrentUserId);
            if (student == null) return NotFound();

            var rcRes = await _examService.GetStudentReportCardAsync(student.StudentId, examTermId);
            if (!rcRes.Success || rcRes.Data == null) return NotFound("Report Card not found.");

            return View(rcRes.Data);
        }

        public async Task<IActionResult> PrintReportCard(int examTermId)
        {
            var student = await _db.Students.FirstOrDefaultAsync(s => s.UserId == CurrentUserId);
            if (student == null) return NotFound();

            var rcRes = await _examService.GetStudentReportCardAsync(student.StudentId, examTermId);
            if (!rcRes.Success || rcRes.Data == null) return NotFound("Report Card not found.");

            return View("PrintReportCard", rcRes.Data);
        }
    }
}
