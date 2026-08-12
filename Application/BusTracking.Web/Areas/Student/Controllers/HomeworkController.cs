namespace BusTracking.Web.Areas.Student.Controllers
{
    [Area("Student"), Authorize(Roles = "Student")]
    public class HomeworkController : Controller
    {
        private readonly IHomeworkService _homeworkService;
        private readonly IAcademicYearService _academicYearService;
        private readonly IWebHostEnvironment _env;

        private int CurrentUserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 1;

        public HomeworkController(
            IHomeworkService homeworkService,
            IAcademicYearService academicYearService,
            IWebHostEnvironment env)
        {
            _homeworkService = homeworkService;
            _academicYearService = academicYearService;
            _env = env;
        }

        public async Task<IActionResult> Index(int? academicYearId, DateTime? fromDate, DateTime? toDate)
        {
            var defaultFrom = fromDate ?? DateTime.Today.AddDays(-7);
            var defaultTo = toDate ?? DateTime.Today;

            var years = await _academicYearService.GetAcademicYearsAsync(1);

            ViewBag.AcademicYears = years;
            ViewBag.SelectedYearId = academicYearId;
            ViewBag.FromDate = defaultFrom;
            ViewBag.ToDate = defaultTo;

            var res = await _homeworkService.GetHomeworksForStudentAsync(CurrentUserId, academicYearId, defaultFrom, defaultTo);
            return View(res.Data ?? new List<HomeworkDto>());
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int homeworkId, string? submissionText, IFormFile? solutionFile)
        {
            string? attachmentUrl = null;
            if (solutionFile != null && solutionFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.ContentRootPath, "media", "submissions");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(solutionFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await solutionFile.CopyToAsync(stream);
                }
                attachmentUrl = $"/media/submissions/{fileName}";
            }

            var dto = new SubmitHomeworkDto
            {
                HomeworkId = homeworkId,
                SubmissionText = submissionText,
                AttachmentUrl = attachmentUrl
            };

            var res = await _homeworkService.SubmitHomeworkAsync(dto, CurrentUserId);
            if (!res.Success) TempData["ErrorMessage"] = res.Message;
            else TempData["SuccessMessage"] = "Homework submitted successfully!";

            return RedirectToAction(nameof(Index));
        }
    }
}
