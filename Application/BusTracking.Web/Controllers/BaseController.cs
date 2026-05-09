using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace BusTracking.Web.Controllers;

public class BaseController : Controller
{
    protected int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

    protected string CurrentUserRole =>
        User.FindFirstValue(ClaimTypes.Role) ?? "";

    protected string CurrentUserEmail =>
        User.FindFirstValue(ClaimTypes.Email) ?? "";
}
