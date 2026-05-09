using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace BusTracking.Web.Controllers;

public abstract class BaseController : Controller
{
    protected int CurrentUserId =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    protected string CurrentUserRole =>
        User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
}
