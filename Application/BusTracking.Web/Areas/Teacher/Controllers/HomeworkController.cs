namespace BusTracking.Web.Areas.Teacher.Controllers
{
    [Area("Teacher"), Authorize(Roles = "Teacher")]
    public class HomeworkController : Controller
    {
        private readonly IHomeworkService _homeworkService;
        private readonly IStandardService _standardService;
        private readonly ISectionService _sectionService;
        private readonly ISubjectService _subjectService;
        private readonly IAcademicYearService _academicYearService;
        private readonly IWebHostEnvironment _env;

        private int CurrentUserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 1;

        public HomeworkController(
            IHomeworkService homeworkService,
            IStandardService standardService,
            ISectionService sectionService,
            ISubjectService subjectService,
            IAcademicYearService academicYearService,
            IWebHostEnvironment env)
        {
            _homeworkService = homeworkService;
            _standardService = standardService;
            _sectionService = sectionService;
            _subjectService = subjectService;
            _academicYearService = academicYearService;
            _env = env;
        }

        public async Task<IActionResult> Index(int? academicYearId, int? standardId, int? sectionId)
        {
            var years = await _academicYearService.GetAcademicYearsAsync(1);
            var activeYear = years.FirstOrDefault(y => y.IsCurrent) ?? years.FirstOrDefault();
            int selectedYearId = academicYearId ?? activeYear?.AcademicYearId ?? 0;

            var standards = (await _standardService.GetActiveStandardsAsync()).Data ?? new();
            int selectedStandardId = standardId ?? 0;

            var sections = selectedStandardId > 0
                ? (await _sectionService.GetSectionsByStandardAsync(selectedStandardId)).Data ?? new()
                : new();
            int selectedSectionId = sectionId ?? 0;

            ViewBag.AcademicYears = years;
            ViewBag.SelectedYearId = selectedYearId;
            ViewBag.Standards = standards;
            ViewBag.SelectedStandardId = selectedStandardId;
            ViewBag.Sections = sections;
            ViewBag.SelectedSectionId = selectedSectionId;

            bool hasFilter = academicYearId.HasValue && academicYearId.Value > 0 && standardId.HasValue && standardId.Value > 0 && sectionId.HasValue && sectionId.Value > 0;
            ViewBag.HasFilter = hasFilter;

            var result = hasFilter
                ? await _homeworkService.GetHomeworksForTeacherAsync(CurrentUserId, selectedYearId, selectedStandardId, selectedSectionId)
                : ApiResponse<List<HomeworkDto>>.Ok(new List<HomeworkDto>());

            return View(result.Data ?? new List<HomeworkDto>());
        }


        public async Task<IActionResult> Create()
        {
            var years = await _academicYearService.GetAcademicYearsAsync(1);
            var activeYear = years.FirstOrDefault(y => y.IsCurrent) ?? years.FirstOrDefault();
            var standards = (await _standardService.GetActiveStandardsAsync()).Data ?? new();
            var subjects = (await _subjectService.GetActiveSubjectsAsync()).Data ?? new();

            ViewBag.AcademicYears = years;
            ViewBag.ActiveYearId = activeYear?.AcademicYearId ?? 0;
            ViewBag.Standards = standards;
            ViewBag.Subjects = subjects;

            return View(new CreateHomeworkDto { DueDate = DateTime.Today.AddDays(1), IsActive = true });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateHomeworkDto dto, IFormFile? attachmentFile)
        {
            if (attachmentFile != null && attachmentFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.ContentRootPath, "media", "homework");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(attachmentFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await attachmentFile.CopyToAsync(stream);
                }
                dto.AttachmentUrl = $"/media/homework/{fileName}";
            }

            var res = await _homeworkService.CreateHomeworkAsync(dto, CurrentUserId);
            if (!res.Success)
            {
                TempData["ErrorMessage"] = res.Message;
                return RedirectToAction(nameof(Create));
            }

            TempData["SuccessMessage"] = dto.IsActive
                ? "Homework created and push notifications sent successfully!"
                : "Homework saved as Draft/Inactive successfully!";
            return RedirectToAction(nameof(Index), new { academicYearId = dto.AcademicYearId, standardId = dto.StandardId, sectionId = dto.SectionId });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var hwRes = await _homeworkService.GetHomeworkByIdAsync(id);
            if (!hwRes.Success || hwRes.Data == null)
            {
                TempData["ErrorMessage"] = "Homework not found.";
                return RedirectToAction(nameof(Index));
            }

            var hw = hwRes.Data;
            var years = await _academicYearService.GetAcademicYearsAsync(1);
            var standards = (await _standardService.GetActiveStandardsAsync()).Data ?? new();
            var sections = (await _sectionService.GetSectionsByStandardAsync(hw.StandardId)).Data ?? new();
            var subjects = (await _subjectService.GetActiveSubjectsAsync()).Data ?? new();

            ViewBag.AcademicYears = years;
            ViewBag.Standards = standards;
            ViewBag.Sections = sections;
            ViewBag.Subjects = subjects;

            var dto = new UpdateHomeworkDto
            {
                HomeworkId = hw.HomeworkId,
                AcademicYearId = hw.AcademicYearId,
                StandardId = hw.StandardId,
                SectionId = hw.SectionId,
                SubjectId = hw.SubjectId,
                Title = hw.Title,
                Description = hw.Description,
                AttachmentUrl = hw.AttachmentUrl,
                DueDate = hw.DueDate,
                IsActive = hw.IsActive
            };

            return View(dto);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateHomeworkDto dto, IFormFile? attachmentFile)
        {
            if (attachmentFile != null && attachmentFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.ContentRootPath, "media", "homework");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(attachmentFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await attachmentFile.CopyToAsync(stream);
                }
                dto.AttachmentUrl = $"/media/homework/{fileName}";
            }

            var res = await _homeworkService.UpdateHomeworkAsync(dto, CurrentUserId);
            if (!res.Success)
            {
                TempData["ErrorMessage"] = res.Message;
                return RedirectToAction(nameof(Edit), new { id = dto.HomeworkId });
            }

            TempData["SuccessMessage"] = "Homework updated successfully!";
            return RedirectToAction(nameof(Index), new { academicYearId = dto.AcademicYearId, standardId = dto.StandardId, sectionId = dto.SectionId });
        }

        public async Task<IActionResult> Submissions(int id)
        {
            var hwRes = await _homeworkService.GetHomeworkByIdAsync(id);
            if (!hwRes.Success || hwRes.Data == null)
            {
                TempData["ErrorMessage"] = "Homework not found.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Homework = hwRes.Data;
            var subRes = await _homeworkService.GetSubmissionsForHomeworkAsync(id);
            return View(subRes.Data ?? new List<HomeworkSubmissionDto>());
        }

        [HttpPost]
        public async Task<IActionResult> Evaluate([FromBody] EvaluateHomeworkSubmissionDto dto)
        {
            var res = await _homeworkService.EvaluateSubmissionAsync(dto, CurrentUserId);
            if (res.Success)
            {
                TempData["SuccessMessage"] = "Submission evaluated successfully.";
            }
            return Json(res);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var res = await _homeworkService.DeleteHomeworkAsync(id, CurrentUserId);
            if (!res.Success) TempData["ErrorMessage"] = res.Message;
            else TempData["SuccessMessage"] = res.Message;

            return RedirectToAction(nameof(Index));
        }
    }
}
