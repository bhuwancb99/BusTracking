namespace BusTracking.API.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    public class LoggerController : ApiBaseController
    {
        private readonly ILogService _log;

        public LoggerController(ILogService log)
        {
            _log = log;
        }

        [HttpPost]
        public async Task<IActionResult> Log([FromBody] LogEntryDto dto)
        {
            int? userId = null;
            string? username = null;
            string? role = null;

            if (User.Identity?.IsAuthenticated == true)
            {
                userId = CurrentUserId;
                username = User.FindFirst(ClaimTypes.Email)?.Value ?? User.Identity.Name;
                role = CurrentUserRole;
            }
            else
            {
                userId = dto.UserId;
                username = dto.Username;
                role = dto.Role;
            }

            await _log.LogAsync(
                platform: dto.Platform,
                exceptionMessage: dto.ExceptionMessage,
                stackTrace: dto.StackTrace,
                requestUrl: dto.RequestUrl,
                userId: userId,
                username: username,
                role: role,
                moduleName: dto.ModuleName,
                actionName: dto.ActionName,
                additionalDetails: dto.AdditionalDetails
            );

            return Ok();
        }
    }
}
