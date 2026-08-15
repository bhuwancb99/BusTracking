namespace BusTracking.Web.Areas.BusCoordinator.Controllers
{
    [Area("BusCoordinator"), Authorize(Roles = "BusCoordinator")]
    public class ExamController : Controller
    {
        private readonly IExamService _examService;
        private readonly IAcademicYearService _academicYearService;
        private readonly IStandardService _standardService;
        private readonly ISubjectService _subjectService;
        private readonly ISectionService _sectionService;

        private bool CheckPermission(string permissionKey)
        {
            return PermissionHelper.Can(User, permissionKey, HttpContext);
        }

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
            if (!CheckPermission("examterm.view")) return RedirectToAction("Index", "AccessDenied");

            var years = await _academicYearService.GetAcademicYearsAsync(1);
            ViewBag.AcademicYears = years;
            ViewBag.SelectedYearId = academicYearId;

            var res = await _examService.GetExamTermsAsync(academicYearId);
            return View(res.Data ?? new());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTerm(CreateExamTermDto dto)
        {
            if (!CheckPermission("examterm.add")) return RedirectToAction("Index", "AccessDenied");

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
            if (!CheckPermission("examterm.edit")) return RedirectToAction("Index", "AccessDenied");

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
            if (!CheckPermission("examterm.delete")) return RedirectToAction("Index", "AccessDenied");

            var res = await _examService.DeleteExamTermAsync(id);
            if (res.Success) TempData["SuccessMessage"] = "Exam Term deleted.";
            else TempData["ErrorMessage"] = res.Message;

            return RedirectToAction(nameof(Terms));
        }

        public async Task<IActionResult> Schedules(int? examTermId, int? standardId, int? sectionId, bool isSubmitted = false)
        {
            if (!CheckPermission("examschedule.view")) return RedirectToAction("Index", "AccessDenied");

            var years = await _academicYearService.GetAcademicYearsAsync(1);
            var activeYear = years.Find(y => y.IsCurrent) ?? years.Find(y => y.IsActive);

            var termsRes = await _examService.GetExamTermsAsync(activeYear?.AcademicYearId);
            var terms = termsRes.Data ?? new();

            var stdsRes = await _standardService.GetActiveStandardsAsync();
            var stds = stdsRes.Data ?? new();

            var subsRes = await _subjectService.GetActiveSubjectsAsync();

            ViewBag.AcademicYears = years;
            ViewBag.ActiveYearId = activeYear?.AcademicYearId;
            ViewBag.ExamTerms = terms;
            ViewBag.SelectedTermId = examTermId;
            ViewBag.Standards = stds;
            ViewBag.SelectedStandardId = standardId;
            ViewBag.Subjects = subsRes.Data ?? new();

            var sectionsRes = standardId.HasValue && standardId.Value > 0
                ? await _sectionService.GetSectionsByStandardAsync(standardId.Value)
                : null;
            ViewBag.Sections = sectionsRes?.Data ?? new List<SectionDto>();
            ViewBag.SelectedSectionId = sectionId;

            var list = new List<ExamScheduleDto>();
            if (examTermId.HasValue && examTermId.Value > 0 && standardId.HasValue && standardId.Value > 0 && sectionId.HasValue)
            {
                var schedulesRes = await _examService.GetExamSchedulesAsync(examTermId, standardId, sectionId);
                list = schedulesRes.Data ?? new();
            }
            else if (isSubmitted || examTermId.HasValue || standardId.HasValue || sectionId.HasValue)
            {
                ViewBag.ValidationMessage = "Please select Exam Term, Class / Standard, and Section to view the datesheet schedule.";
            }

            return View(list);
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
            if (!CheckPermission("examschedule.add")) return RedirectToAction("Index", "AccessDenied");

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
            if (!CheckPermission("examschedule.edit")) return RedirectToAction("Index", "AccessDenied");

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
            if (!CheckPermission("examschedule.delete")) return RedirectToAction("Index", "AccessDenied");

            var res = await _examService.DeleteExamScheduleAsync(id);
            if (res.Success) TempData["SuccessMessage"] = "Schedule item removed.";
            else TempData["ErrorMessage"] = res.Message;

            return RedirectToAction(nameof(Schedules));
        }
    }
}
