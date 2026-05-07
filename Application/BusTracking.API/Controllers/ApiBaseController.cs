using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BusTracking.API.Controllers
{
    [ApiController]
    public abstract class ApiBaseController : ControllerBase
    {
        protected int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        protected string CurrentUserRole =>
            User.FindFirstValue(ClaimTypes.Role) ?? "";
    }
}
