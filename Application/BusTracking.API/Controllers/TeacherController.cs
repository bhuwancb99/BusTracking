namespace BusTracking.API.Controllers
{
    [Authorize(Roles = "Teacher")]
    [Route("api/teacher")]
    public class TeacherController : ApiBaseController
    {
        private readonly ITeacherService _teacherService;
        private readonly INotificationService _notificationService;
        private readonly IAttendanceService _attendanceService;
        private readonly ISectionService _sectionService;
        private readonly IClassMappingService _classMappingService;
        private readonly IAcademicYearService _academicYearService;
        private readonly IStandardService _standardService;
        private readonly ISubjectService _subjectService;
        private readonly IHomeworkService _homeworkService;
        private readonly IExamService _examService;
        private readonly IWebHostEnvironment _env;

        public TeacherController(
            ITeacherService teacherService,
            INotificationService notificationService,
            IAttendanceService attendanceService,
            ISectionService sectionService,
            IClassMappingService classMappingService,
            IAcademicYearService academicYearService,
            IStandardService standardService,
            ISubjectService subjectService,
            IHomeworkService homeworkService,
            IExamService examService,
            IWebHostEnvironment env)
        {
            _teacherService = teacherService;
            _notificationService = notificationService;
            _attendanceService = attendanceService;
            _sectionService = sectionService;
            _classMappingService = classMappingService;
            _academicYearService = academicYearService;
            _standardService = standardService;
            _subjectService = subjectService;
            _homeworkService = homeworkService;
            _examService = examService;
            _env = env;
        }


        /// <summary>
        /// Gets active academic years for Teacher.
        /// </summary>
        [HttpGet("academicyears")]
        public async Task<IActionResult> GetAcademicYears()
        {
            var schoolId = CurrentSchoolId ?? 1;
            var years = await _academicYearService.GetAcademicYearsAsync(schoolId);
            return Ok(ApiResponse<List<AcademicYearDto>>.Ok(years));
        }

        /// <summary>
        /// Switch active academic session for Teacher's school.
        /// </summary>
        [HttpPost("session/switch/{id:int}")]
        public async Task<IActionResult> SwitchSession(int id)
        {
            int schoolId = CurrentSchoolId ?? 1;
            var userName = User.Identity?.Name ?? User.GetFullName() ?? "Teacher";
            var result = await _academicYearService.SetActiveAcademicYearAsync(schoolId, id, userName);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Gets standards/classes for Teacher.
        /// </summary>
        [HttpGet("standards")]
        public async Task<IActionResult> GetStandards([FromQuery] string? search, [FromQuery] int page = 1)
        {
            var result = await _standardService.GetAllAsync(search, true, page);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Gets sections by standard ID for Teacher.
        /// </summary>
        [HttpGet("sections/by-standard/{standardId:int}")]
        public async Task<IActionResult> GetSectionsByStandard(int standardId)
        {
            var result = await _sectionService.GetSectionsByStandardAsync(standardId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Gets subjects for Teacher.
        /// </summary>
        [HttpGet("subjects")]
        public async Task<IActionResult> GetSubjects([FromQuery] string? search, [FromQuery] int page = 1)
        {
            var result = await _subjectService.GetAllAsync(search, true, page);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Gets the logged-in Teacher's profile details.
        /// </summary>
        [HttpGet("profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            var result = await _teacherService.GetTeacherByUserIdAsync(CurrentUserId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Gets the logged-in Teacher's notifications.
        /// </summary>
        [HttpGet("notifications")]
        public async Task<IActionResult> GetMyNotifications()
        {
            var result = await _notificationService.GetUserNotificationsAsync(CurrentUserId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Gets students for attendance in a class section.
        /// </summary>
        [HttpGet("attendance/students")]
        public async Task<IActionResult> GetStudentsForAttendance(
            [FromQuery] int academicYearId,
            [FromQuery] int standardId,
            [FromQuery] int? sectionId,
            [FromQuery] DateTime date)
        {
            var result = await _attendanceService.GetStudentsForAttendanceAsync(academicYearId, standardId, sectionId, date);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Teacher submits manual checklist attendance batch.
        /// </summary>
        [HttpPost("attendance/manual-batch")]
        public async Task<IActionResult> SaveManualAttendanceBatch([FromBody] ManualAttendanceBatchDto dto)
        {
            var result = await _attendanceService.SaveManualAttendanceBatchAsync(dto, CurrentUserId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Teacher submits classroom face recognition scan.
        /// </summary>
        [HttpPost("attendance/face-scan")]
        public async Task<IActionResult> ProcessFaceScanAttendance([FromBody] FaceAttendanceScanRequestDto dto)
        {
            var result = await _attendanceService.ProcessFaceScanAttendanceAsync(dto, CurrentUserId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Gets daily/monthly classroom attendance report for teacher's class.
        /// </summary>
        [HttpGet("attendance/report")]
        public async Task<IActionResult> GetAttendanceReport(
            [FromQuery] int academicYearId,
            [FromQuery] int standardId,
            [FromQuery] int? sectionId,
            [FromQuery] DateTime date)
        {
            var result = await _attendanceService.GetAttendanceReportAsync(academicYearId, standardId, sectionId, date);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // ── TEACHER HOMEWORK ENDPOINTS ──────────────────────────────────────────

        /// <summary>
        /// Teacher: List homework assignments filtered by session, standard, section.
        /// </summary>
        [HttpGet("homework/list")]
        public async Task<IActionResult> GetHomeworksForTeacher(
            [FromQuery] int? academicYearId,
            [FromQuery] int? standardId,
            [FromQuery] int? sectionId)
        {
            var res = await _homeworkService.GetHomeworksForTeacherAsync(CurrentUserId, academicYearId, standardId, sectionId);
            return res.Success ? Ok(res) : BadRequest(res);
        }

        /// <summary>
        /// Teacher: Get single homework assignment details by ID.
        /// </summary>
        [HttpGet("homework/{id:int}")]
        public async Task<IActionResult> GetHomeworkById(int id)
        {
            var res = await _homeworkService.GetHomeworkByIdAsync(id);
            return res.Success ? Ok(res) : NotFound(res);
        }

        /// <summary>
        /// Teacher: Create a new homework assignment (supports file attachment upload).
        /// </summary>
        [HttpPost("homework/create")]
        public async Task<IActionResult> CreateHomework([FromForm] CreateHomeworkDto dto, IFormFile? attachmentFile)
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
            return res.Success ? Ok(res) : BadRequest(res);
        }

        /// <summary>
        /// Teacher: Update an existing homework assignment (supports replacing file attachment).
        /// </summary>
        [HttpPut("homework/update/{id:int}")]
        public async Task<IActionResult> UpdateHomework(int id, [FromForm] UpdateHomeworkDto dto, IFormFile? attachmentFile)
        {
            dto.HomeworkId = id;
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
            return res.Success ? Ok(res) : BadRequest(res);
        }

        /// <summary>
        /// Teacher: Hard delete homework if 0 student submissions exist.
        /// </summary>
        [HttpDelete("homework/delete/{id:int}")]
        public async Task<IActionResult> DeleteHomework(int id)
        {
            var res = await _homeworkService.DeleteHomeworkAsync(id, CurrentUserId);
            return res.Success ? Ok(res) : BadRequest(res);
        }

        /// <summary>
        /// Teacher: Get student submissions for a specific homework assignment.
        /// </summary>
        [HttpGet("homework/submissions/{id:int}")]
        public async Task<IActionResult> GetSubmissionsForHomework(int id)
        {
            var res = await _homeworkService.GetSubmissionsForHomeworkAsync(id);
            return res.Success ? Ok(res) : BadRequest(res);
        }

        /// <summary>
        /// Teacher: Grade/evaluate a student's homework submission.
        /// </summary>
        [HttpPost("homework/evaluate")]
        public async Task<IActionResult> EvaluateSubmission([FromBody] EvaluateHomeworkSubmissionDto dto)
        {
            var res = await _homeworkService.EvaluateSubmissionAsync(dto, CurrentUserId);
            return res.Success ? Ok(res) : BadRequest(res);
        }

        [HttpGet("exam/terms")]
        public async Task<IActionResult> GetTerms([FromQuery] int? academicYearId)
        {
            if (!academicYearId.HasValue || academicYearId.Value <= 0)
            {
                var years = await _academicYearService.GetAcademicYearsAsync(CurrentSchoolId ?? 1);
                var activeYear = years.Find(y => y.IsCurrent) ?? years.Find(y => y.IsActive);
                academicYearId = activeYear?.AcademicYearId;
            }

            var res = await _examService.GetExamTermsAsync(academicYearId);
            return res.Success ? Ok(res) : BadRequest(res);
        }

        [HttpGet("exam/schedules")]
        public async Task<IActionResult> GetSchedules([FromQuery] int? examTermId, [FromQuery] int? standardId)
        {
            var res = await _examService.GetExamSchedulesAsync(examTermId, standardId);
            return res.Success ? Ok(res) : BadRequest(res);
        }

        [HttpGet("exam/marks-grid")]
        public async Task<IActionResult> GetMarksGrid([FromQuery] int examScheduleId, [FromQuery] int? sectionId)
        {
            if (examScheduleId <= 0)
            {
                return Ok(ApiResponse<List<StudentMarksGridItemDto>>.Fail("Valid examScheduleId is required."));
            }

            var res = await _examService.GetStudentMarksGridAsync(examScheduleId, sectionId);
            return res.Success ? Ok(res) : BadRequest(res);
        }

        [HttpPost("exam/save-marks")]
        public async Task<IActionResult> SaveMarks([FromBody] SaveStudentMarksGridDto dto)
        {
            if (dto == null || dto.ExamScheduleId <= 0)
            {
                return Ok(ApiResponse<bool>.Fail("Invalid schedule selection."));
            }

            var res = await _examService.SaveStudentMarksGridAsync(dto, CurrentUserId);
            return res.Success ? Ok(res) : BadRequest(res);
        }
    }
}

