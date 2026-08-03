namespace BusTracking.API.Controllers
{
    [ApiController]
    [Route("api/teacher")]
    [Authorize(Roles = "Teacher")]
    public class TeacherController : ControllerBase
    {
        private readonly ITeacherService _teacherService;
        private readonly INotificationService _notificationService;

        private int CurrentUserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : 0;

        public TeacherController(ITeacherService teacherService, INotificationService notificationService)
        {
            _teacherService = teacherService;
            _notificationService = notificationService;
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
    }
}
