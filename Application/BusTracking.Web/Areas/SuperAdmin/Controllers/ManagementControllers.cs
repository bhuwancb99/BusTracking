using BusTracking.Common.DTOs.Availability;
using BusTracking.Common.DTOs.Bus;
using BusTracking.Common.DTOs.Driver;
using BusTracking.Common.DTOs.Parent;
using BusTracking.Common.DTOs.Route;
using BusTracking.Common.DTOs.Stop;
using BusTracking.Common.DTOs.Student;
using BusTracking.Common.DTOs.SubAdmin;
using BusTracking.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BusTracking.Web.Areas.SuperAdmin.Controllers;

// ── Bus ───────────────────────────────────────────────────────────────
[Area("SuperAdmin"), Authorize(Roles = "SuperAdmin")]
public class BusController : Controller
{
    private readonly IBusService _bus;
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    public BusController(IBusService bus) => _bus = bus;

    public async Task<IActionResult> Index(int page = 1, string? search = null)
    { ViewBag.Search = search; var r = await _bus.GetAllAsync(page, 10, search); return View(r.Data); }

    public async Task<IActionResult> Details(int id)
    { var r = await _bus.GetByIdAsync(id); return r.Success ? View(r.Data) : NotFound(); }

    [HttpGet] public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBusDto m)
    {
        if (!ModelState.IsValid) return View(m);
        var r = await _bus.CreateAsync(m, UserId);
        if (!r.Success) { ModelState.AddModelError("", r.Message); return View(m); }
        TempData["SuccessMessage"] = r.Message; return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var r = await _bus.GetByIdAsync(id); if (!r.Success) return NotFound();
        ViewBag.BusId = id;
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

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    { await _bus.DeleteAsync(id); TempData["SuccessMessage"] = "Bus deleted."; return RedirectToAction(nameof(Index)); }
}

// ── Driver ────────────────────────────────────────────────────────────
[Area("SuperAdmin"), Authorize(Roles = "SuperAdmin")]
public class DriverController : Controller
{
    private readonly IDriverService _driver;
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    public DriverController(IDriverService driver) => _driver = driver;

    public async Task<IActionResult> Index(int page = 1, string? search = null)
    { ViewBag.Search = search; var r = await _driver.GetAllAsync(page, 10, search); return View(r.Data); }

    public async Task<IActionResult> Details(int id)
    { var r = await _driver.GetByIdAsync(id); return r.Success ? View(r.Data) : NotFound(); }

    [HttpGet] public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateDriverDto m)
    {
        if (!ModelState.IsValid) return View(m);
        var r = await _driver.CreateAsync(m, UserId);
        if (!r.Success) { ModelState.AddModelError("", r.Message); return View(m); }
        TempData["SuccessMessage"] = r.Message; return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var r = await _driver.GetByIdAsync(id); if (!r.Success) return NotFound();
        ViewBag.DriverId = id;
        return View(new UpdateDriverDto { FullName = r.Data!.FullName, PhoneNumber = r.Data.PhoneNumber, LicenseNumber = r.Data.LicenseNumber });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateDriverDto m)
    {
        if (!ModelState.IsValid) { ViewBag.DriverId = id; return View(m); }
        var r = await _driver.UpdateAsync(id, m);
        if (!r.Success) { ModelState.AddModelError("", r.Message); ViewBag.DriverId = id; return View(m); }
        TempData["SuccessMessage"] = r.Message; return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    { await _driver.DeleteAsync(id); TempData["SuccessMessage"] = "Driver deleted."; return RedirectToAction(nameof(Index)); }
}

// ── Student ───────────────────────────────────────────────────────────
[Area("SuperAdmin"), Authorize(Roles = "SuperAdmin")]
public class StudentController : Controller
{
    private readonly IStudentService _student;
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    public StudentController(IStudentService s) => _student = s;

    public async Task<IActionResult> Index(int page = 1, string? search = null)
    { ViewBag.Search = search; var r = await _student.GetAllAsync(page, 10, search); return View(r.Data); }

    [HttpGet] public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateStudentDto m)
    {
        if (!ModelState.IsValid) return View(m);
        var r = await _student.CreateAsync(m, UserId);
        if (!r.Success) { ModelState.AddModelError("", r.Message); return View(m); }
        TempData["SuccessMessage"] = r.Message; return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var r = await _student.GetByIdAsync(id); if (!r.Success) return NotFound();
        ViewBag.StudentId = id;
        return View(new UpdateStudentDto { FullName = r.Data!.FullName, Standard = r.Data.Standard });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateStudentDto m)
    {
        if (!ModelState.IsValid) { ViewBag.StudentId = id; return View(m); }
        var r = await _student.UpdateAsync(id, m);
        if (!r.Success) { ModelState.AddModelError("", r.Message); ViewBag.StudentId = id; return View(m); }
        TempData["SuccessMessage"] = r.Message; return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    { await _student.DeleteAsync(id); TempData["SuccessMessage"] = "Student deleted."; return RedirectToAction(nameof(Index)); }

    public async Task<IActionResult> Availability(int studentId)
    { var r = await _student.GetAvailabilitiesAsync(studentId); ViewBag.StudentId = studentId; return View(r.Data); }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetAvailability(CreateAvailabilityDto m)
    {
        var r = await _student.SetAvailabilityAsync(m, UserId);
        TempData[r.Success ? "SuccessMessage" : "ErrorMessage"] = r.Message;
        return RedirectToAction(nameof(Availability), new { studentId = m.StudentId });
    }
}

// ── Parent ────────────────────────────────────────────────────────────
[Area("SuperAdmin"), Authorize(Roles = "SuperAdmin")]
public class ParentController : Controller
{
    private readonly IParentService _parent;
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    public ParentController(IParentService p) => _parent = p;

    public async Task<IActionResult> Index(int page = 1, string? search = null)
    { ViewBag.Search = search; var r = await _parent.GetAllAsync(page, 10, search); return View(r.Data); }

    public async Task<IActionResult> Details(int id)
    { var r = await _parent.GetByIdAsync(id); return r.Success ? View(r.Data) : NotFound(); }

    [HttpGet] public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateParentDto m)
    {
        if (!ModelState.IsValid) return View(m);
        var r = await _parent.CreateAsync(m, UserId);
        if (!r.Success) { ModelState.AddModelError("", r.Message); return View(m); }
        TempData["SuccessMessage"] = r.Message; return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var r = await _parent.GetByIdAsync(id); if (!r.Success) return NotFound();
        ViewBag.ParentId = id;
        return View(new UpdateParentDto { FullName = r.Data!.FullName, PhoneNumber = r.Data.PhoneNumber });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateParentDto m)
    {
        if (!ModelState.IsValid) { ViewBag.ParentId = id; return View(m); }
        var r = await _parent.UpdateAsync(id, m);
        if (!r.Success) { ModelState.AddModelError("", r.Message); ViewBag.ParentId = id; return View(m); }
        TempData["SuccessMessage"] = r.Message; return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    { await _parent.DeleteAsync(id); TempData["SuccessMessage"] = "Parent deleted."; return RedirectToAction(nameof(Index)); }
}

// ── Route ─────────────────────────────────────────────────────────────
[Area("SuperAdmin"), Authorize(Roles = "SuperAdmin")]
public class RouteController : Controller
{
    private readonly IRouteService _route;
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    public RouteController(IRouteService r) => _route = r;

    public async Task<IActionResult> Index(int page = 1, string? search = null)
    { ViewBag.Search = search; var r = await _route.GetAllAsync(page, 10, search); return View(r.Data); }

    public async Task<IActionResult> Details(int id)
    { var r = await _route.GetByIdAsync(id); return r.Success ? View(r.Data) : NotFound(); }

    [HttpGet] public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRouteDto m)
    {
        if (!ModelState.IsValid) return View(m);
        var r = await _route.CreateAsync(m, UserId);
        if (!r.Success) { ModelState.AddModelError("", r.Message); return View(m); }
        TempData["SuccessMessage"] = r.Message; return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var r = await _route.GetByIdAsync(id); if (!r.Success) return NotFound();
        ViewBag.RouteId = id;
        return View(new UpdateRouteDto { RouteName = r.Data!.RouteName, RouteCode = r.Data.RouteCode, MorningTime = r.Data.MorningTime, EveningTime = r.Data.EveningTime, Description = r.Data.Description });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateRouteDto m)
    {
        if (!ModelState.IsValid) { ViewBag.RouteId = id; return View(m); }
        var r = await _route.UpdateAsync(id, m);
        if (!r.Success) { ModelState.AddModelError("", r.Message); ViewBag.RouteId = id; return View(m); }
        TempData["SuccessMessage"] = r.Message; return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    { await _route.DeleteAsync(id); TempData["SuccessMessage"] = "Route deleted."; return RedirectToAction(nameof(Index)); }

    [HttpPost]
    public async Task<IActionResult> AddStop([FromBody] CreateStopDto dto)
    { var r = await _route.AddStopAsync(dto); return Json(new { r.Success, r.Message }); }

    [HttpPost]
    public async Task<IActionResult> DeleteStop(int stopId)
    { var r = await _route.DeleteStopAsync(stopId); return Json(new { r.Success, r.Message }); }
}

// ── SubAdmin ──────────────────────────────────────────────────────────
[Area("SuperAdmin"), Authorize(Roles = "SuperAdmin")]
public class SubAdminController : Controller
{
    private readonly ISubAdminService _sa;
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    public SubAdminController(ISubAdminService sa) => _sa = sa;

    public async Task<IActionResult> Index(int page = 1, string? search = null)
    { ViewBag.Search = search; var r = await _sa.GetAllAsync(page, 10, search); return View(r.Data); }

    public async Task<IActionResult> Details(int id)
    { var r = await _sa.GetByIdAsync(id); return r.Success ? View(r.Data) : NotFound(); }

    [HttpGet] public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateSubAdminDto m)
    {
        if (!ModelState.IsValid) return View(m);
        var r = await _sa.CreateAsync(m, UserId);
        if (!r.Success) { ModelState.AddModelError("", r.Message); return View(m); }
        TempData["SuccessMessage"] = r.Message; return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var r = await _sa.GetByIdAsync(id); if (!r.Success) return NotFound();
        ViewBag.SubAdminId = id;
        return View(new UpdateSubAdminDto { FullName = r.Data!.FullName, PhoneNumber = r.Data.PhoneNumber });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateSubAdminDto m)
    {
        if (!ModelState.IsValid) { ViewBag.SubAdminId = id; return View(m); }
        var r = await _sa.UpdateAsync(id, m);
        if (!r.Success) { ModelState.AddModelError("", r.Message); ViewBag.SubAdminId = id; return View(m); }
        TempData["SuccessMessage"] = r.Message; return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    { await _sa.DeleteAsync(id); TempData["SuccessMessage"] = "Coordinator deleted."; return RedirectToAction(nameof(Index)); }

    [HttpPost]
    public async Task<IActionResult> ToggleActive(int id)
    { var r = await _sa.ToggleActiveAsync(id); return Json(new { r.Success, r.Message }); }
}

// ── Feedback ──────────────────────────────────────────────────────────
[Area("SuperAdmin"), Authorize(Roles = "SuperAdmin")]
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

// ── Notification ──────────────────────────────────────────────────────
[Area("SuperAdmin"), Authorize(Roles = "SuperAdmin")]
public class NotificationController : Controller
{
    private readonly INotificationService _notif;
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    public NotificationController(INotificationService n) => _notif = n;

    public async Task<IActionResult> Index()
    { var r = await _notif.GetUserNotificationsAsync(UserId); return View(r.Data); }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(int id)
    { await _notif.MarkAsReadAsync(id, UserId); return RedirectToAction(nameof(Index)); }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead()
    { await _notif.MarkAllAsReadAsync(UserId); return RedirectToAction(nameof(Index)); }
}
