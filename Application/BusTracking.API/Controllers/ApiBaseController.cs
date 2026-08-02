namespace BusTracking.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    public abstract class ApiBaseController : ControllerBase
    {
        protected int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        protected string CurrentUserRole =>
            User.FindFirstValue(ClaimTypes.Role) ?? "";
        protected string CurrentTimeZoneInfoId =>
            User.FindFirstValue("time_zone_id") ?? User.FindFirstValue("TimeZoneInfoId") ?? "India Standard Time";
    }
}
