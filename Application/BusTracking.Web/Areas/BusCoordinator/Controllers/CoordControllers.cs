using BusTracking.Common.DTOs.Bus;
using BusTracking.Common.DTOs.Student;
using BusTracking.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BusTracking.Web.Areas.BusCoordinator.Controllers;

[Area("BusCoordinator"), Authorize(Roles = "BusCoordinator")]
public class DashboardController : Controller
{
    private readonly IDashboardService _dash;
    public DashboardController(IDashboardService d) => _dash = d;
    public async Task<IActionResult> Index()
    { var r = await _dash.GetSummaryAsync(); return View(r.Data); }
}

[Area("BusCoordinator"), Authorize(Roles = "BusCoordinator")]
public class BusController : Controller
{
    private readonly IBusService _bus;
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    public BusController(IBusService b) => _bus = b;
    public async Task<IActionResult> Index(int page = 1, string? search = null)
    { ViewBag.Search = search; var r = await _bus.GetAllAsync(page, 10, search); return View(r.Data); }
    [HttpGet] public IActionResult Create() => View();
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBusDto m)
    {
        if (!ModelState.IsValid) return View(m); var r = await _bus.CreateAsync(m, UserId);
        if (!r.Success) { ModelState.AddModelError("", r.Message); return View(m); }
        TempData["SuccessMessage"] = r.Message; return RedirectToAction(nameof(Index));
    }
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var r = await _bus.GetByIdAsync(id); if (!r.Success) return NotFound(); ViewBag.BusId = id;
        return View(new UpdateBusDto { BusName = r.Data!.BusName, BusNumber = r.Data.BusNumber, RouteId = r.Data.RouteId, Capacity = r.Data.Capacity });
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateBusDto m)
    {
        if (!ModelState.IsValid) { ViewBag.BusId = id; return View(m); }
        var r = await _bus.UpdateAsync(id, m);
        if (!r.Success) { ModelState.AddModelError("", r.Message); ViewBag.BusId = id; return View(m); }
        TempData["SuccessMessage"] = r.Message; return RedirectToAction(nameof(Index));
    }
}

[Area("BusCoordinator"), Authorize(Roles = "BusCoordinator")]
public class StudentController : Controller
{
    private readonly IStudentService _s;
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    public StudentController(IStudentService s) => _s = s;
    public async Task<IActionResult> Index(int page = 1, string? search = null)
    { ViewBag.Search = search; var r = await _s.GetAllAsync(page, 10, search); return View(r.Data); }
    [HttpGet] public IActionResult Create() => View();
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateStudentDto m)
    {
        if (!ModelState.IsValid) return View(m); var r = await _s.CreateAsync(m, UserId);
        if (!r.Success) { ModelState.AddModelError("", r.Message); return View(m); }
        TempData["SuccessMessage"] = r.Message; return RedirectToAction(nameof(Index));
    }
}

[Area("BusCoordinator"), Authorize(Roles = "BusCoordinator")]
public class FeedbackController : Controller
{
    private readonly IFeedbackService _fb;
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    public FeedbackController(IFeedbackService fb) => _fb = fb;
    public async Task<IActionResult> Index(int page = 1, string? status = null)
    { ViewBag.Status = status; var r = await _fb.GetAllAsync(page, 10, status); return View(r.Data); }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var r = await _fb.UpdateStatusAsync(id, status, UserId);
        TempData[r.Success ? "SuccessMessage" : "ErrorMessage"] = r.Message;
        return RedirectToAction(nameof(Index));
    }
}
