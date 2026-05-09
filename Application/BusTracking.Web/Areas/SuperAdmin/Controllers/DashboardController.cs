using BusTracking.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusTracking.Web.Areas.SuperAdmin.Controllers;

[Area("SuperAdmin")]
[Authorize(Roles = "SuperAdmin")]
public class DashboardController : Controller
{
    private readonly IDashboardService _dash;
    public DashboardController(IDashboardService dash) => _dash = dash;

    public async Task<IActionResult> Index()
    {
        var r = await _dash.GetSummaryAsync();
        return View(r.Data);
    }
}
