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

        public TeacherController(
            ITeacherService teacherService,
            INotificationService notificationService,
            IAttendanceService attendanceService,
            ISectionService sectionService,
            IClassMappingService classMappingService)
        {
            _teacherService = teacherService;
            _notificationService = notificationService;
            _attendanceService = attendanceService;
            _sectionService = sectionService;
            _classMappingService = classMappingService;
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
    }
}
