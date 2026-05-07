using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BusTracking.Web.Controllers
{
    public class BaseController : Controller
    {
        protected int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        protected string CurrentUserRole =>
            User.FindFirstValue(ClaimTypes.Role) ?? "";

        protected string CurrentUserEmail =>
            User.FindFirstValue(ClaimTypes.Email) ?? "";
    }
}
