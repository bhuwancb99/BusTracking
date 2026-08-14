namespace BusTracking.Web.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin"), Authorize(Roles = "SuperAdmin")]
    public class ExamController : Controller
    {
        private readonly IExamService _examService;
        private readonly IAcademicYearService _academicYearService;
        private readonly IStandardService _standardService;
        private readonly ISubjectService _subjectService;
        private readonly ISectionService _sectionService;

        public ExamController(
            IExamService examService,
            IAcademicYearService academicYearService,
            IStandardService standardService,
            ISubjectService subjectService,
            ISectionService sectionService)
        {
            _examService = examService;
            _academicYearService = academicYearService;
            _standardService = standardService;
            _subjectService = subjectService;
            _sectionService = sectionService;
        }

        public async Task<IActionResult> Terms(int? academicYearId)
        {
            var years = await _academicYearService.GetAcademicYearsAsync(1);
            ViewBag.AcademicYears = years;
            ViewBag.SelectedYearId = academicYearId;

            var res = await _examService.GetExamTermsAsync(academicYearId);
            return View(res.Data ?? new());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTerm(CreateExamTermDto dto)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fill in all required fields.";
                return RedirectToAction(nameof(Terms));
            }

            var res = await _examService.CreateExamTermAsync(dto);
            if (res.Success) TempData["SuccessMessage"] = "Exam Term created successfully.";
            else TempData["ErrorMessage"] = res.Message;

            return RedirectToAction(nameof(Terms));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTerm(UpdateExamTermDto dto)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fill in all required fields for editing term.";
                return RedirectToAction(nameof(Terms));
            }

            var res = await _examService.UpdateExamTermAsync(dto.ExamTermId, dto);
            if (res.Success) TempData["SuccessMessage"] = "Exam Term updated successfully.";
            else TempData["ErrorMessage"] = res.Message;

            return RedirectToAction(nameof(Terms));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTerm(int id)
        {
            var res = await _examService.DeleteExamTermAsync(id);
            if (res.Success) TempData["SuccessMessage"] = "Exam Term deleted.";
            else TempData["ErrorMessage"] = res.Message;

            return RedirectToAction(nameof(Terms));
        }

        public async Task<IActionResult> Schedules(int? examTermId, int? standardId)
        {
            var years = await _academicYearService.GetAcademicYearsAsync(1);
            var activeYear = years.Find(y => y.IsCurrent) ?? years.Find(y => y.IsActive);

            var termsRes = await _examService.GetExamTermsAsync(activeYear?.AcademicYearId);
            var terms = termsRes.Data ?? new();

            var stdsRes = await _standardService.GetActiveStandardsAsync();
            var stds = stdsRes.Data ?? new();

            var subsRes = await _subjectService.GetActiveSubjectsAsync();

            int? effectiveTermId = examTermId ?? terms.FirstOrDefault()?.ExamTermId;
            int? effectiveStandardId = standardId ?? stds.FirstOrDefault()?.StandardId;

            int? queryTermId = effectiveTermId == 0 ? null : effectiveTermId;
            int? queryStandardId = effectiveStandardId == 0 ? null : effectiveStandardId;

            ViewBag.AcademicYears = years;
            ViewBag.ActiveYearId = activeYear?.AcademicYearId;
            ViewBag.ExamTerms = terms;
            ViewBag.SelectedTermId = effectiveTermId;
            ViewBag.Standards = stds;
            ViewBag.SelectedStandardId = effectiveStandardId;
            ViewBag.Subjects = subsRes.Data ?? new();

            var schedulesRes = await _examService.GetExamSchedulesAsync(queryTermId, queryStandardId);
            return View(schedulesRes.Data ?? new());
        }


        [HttpGet]
        public async Task<IActionResult> GetSectionsByStandard(int standardId)
        {
            var res = await _sectionService.GetSectionsByStandardAsync(standardId);
            return Json(res.Data ?? new());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSchedule(CreateExamScheduleDto dto)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fill in all required schedule fields.";
                return RedirectToAction(nameof(Schedules));
            }

            var res = await _examService.CreateExamScheduleAsync(dto);
            if (res.Success) TempData["SuccessMessage"] = "Exam Date Schedule added.";
            else TempData["ErrorMessage"] = res.Message;

            return RedirectToAction(nameof(Schedules));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSchedule(UpdateExamScheduleDto dto)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fill in all required schedule fields.";
                return RedirectToAction(nameof(Schedules));
            }

            var res = await _examService.UpdateExamScheduleAsync(dto.ExamScheduleId, dto);
            if (res.Success) TempData["SuccessMessage"] = "Exam schedule item updated.";
            else TempData["ErrorMessage"] = res.Message;

            return RedirectToAction(nameof(Schedules));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSchedule(int id)

        {
            var res = await _examService.DeleteExamScheduleAsync(id);
            if (res.Success) TempData["SuccessMessage"] = "Schedule item removed.";
            else TempData["ErrorMessage"] = res.Message;

            return RedirectToAction(nameof(Schedules));
        }
    }
}
