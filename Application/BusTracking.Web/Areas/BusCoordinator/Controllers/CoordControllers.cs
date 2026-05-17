using BusTracking.Common.DTOs.Assign;
using BusTracking.Common.DTOs.Availability;
using BusTracking.Common.DTOs.Bus;
using BusTracking.Common.DTOs.Driver;
using BusTracking.Common.DTOs.Parent;
using BusTracking.Common.DTOs.Route;
using BusTracking.Common.DTOs.Stop;
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
    {
        var r = await _dash.GetSummaryAsync();
        return View(r.Data);
    }
}

[Area("BusCoordinator"), Authorize(Roles = "BusCoordinator")]
public class BusController : Controller
{
    private readonly IBusService _bus;
    private readonly IRouteService _route;
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    public BusController(IBusService b, IRouteService r)
    {
        _bus = b;
        _route = r;
    }

    public async Task<IActionResult> Index(int page = 1, string? search = null, string? status = "Active")
    {
        ViewBag.Search = search; ViewBag.Status = status;
        var r0 = await _bus.GetAllAsync(page, 10, search, status);
        return View(r0.Data);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadRoutes();
        return View(new CreateBusDto());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBusDto m)
    {
        if (!ModelState.IsValid)
        {
            await LoadRoutes();
            return View(m);
        }
        var r = await _bus.CreateAsync(m, UserId);
        if (!r.Success)
        {
            ModelState.AddModelError("", r.Message);
            await LoadRoutes();
            return View(m);
        }
        TempData["SuccessMessage"] = r.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var r = await _bus.GetByIdAsync(id);
        if (!r.Success)
            return NotFound();
        ViewBag.BusId = id;
        await LoadRoutes(r.Data!.RouteId);
        return View(new UpdateBusDto
        {
            BusName = r.Data.BusName,
            BusNumber = r.Data.BusNumber,
            RouteId = r.Data.RouteId,
            Capacity = r.Data.Capacity,
            IsActive = r.Data.IsActive
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateBusDto m)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.BusId = id;
            await LoadRoutes(m.RouteId);
            return View(m);
        }
        var r = await _bus.UpdateAsync(id, m);
        if (!r.Success)
        {
            ModelState.AddModelError("", r.Message);
            ViewBag.BusId = id;
            await LoadRoutes(m.RouteId);
            return View(m);
        }
        TempData["SuccessMessage"] = r.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _bus.DeleteAsync(id);
        TempData["SuccessMessage"] = "Marked inactive.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadRoutes(int? selectedId = null)
    {
        var routes = await _route.GetAllAsync(1, 100, null);
        ViewBag.Routes = (routes.Data?.Items ?? [])
            .Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = r.RouteId.ToString(),
                Text = $"{r.RouteName} ({r.RouteCode})",
                Selected = r.RouteId == selectedId
            }).ToList();
    }
}

[Area("BusCoordinator"), Authorize(Roles = "BusCoordinator")]
public class DriverController : Controller
{
    private readonly IDriverService _driver;
    private readonly IBusService _bus;
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    public DriverController(IDriverService driver, IBusService bus)
    {
        _driver = driver;
        _bus = bus;
    }

    public async Task<IActionResult> Index(int page = 1, string? search = null, string? status = "Active")
    {
        ViewBag.Search = search; ViewBag.Status = status;
        var r = await _driver.GetAllAsync(page, 10, search, status);
        return View(r.Data);
    }

    public async Task<IActionResult> Details(int id)
    {
        var r = await _driver.GetByIdAsync(id);
        return r.Success ? View(r.Data) : NotFound();
    }

    [HttpGet] public IActionResult Create() => View();
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateDriverDto m)
    {
        if (!ModelState.IsValid)
            return View(m);
        var r = await _driver.CreateAsync(m, UserId);
        if (!r.Success)
        {
            ModelState.AddModelError("", r.Message);
            return View(m);
        }
        TempData["CreatedUser"] = System.Text.Json.JsonSerializer.Serialize(r.Data);
        TempData["SuccessMessage"] = "Driver created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var r = await _driver.GetByIdAsync(id);
        if (!r.Success)
            return NotFound();
        ViewBag.DriverId = id;
        return View(new UpdateDriverDto
        {
            FullName = r.Data!.FullName,
            PhoneNumber = r.Data.PhoneNumber,
            LicenseNumber = r.Data.LicenseNumber,
            LicenseExpiry = r.Data.LicenseExpiry,
            BusId = r.Data.BusId,
            IsActive = r.Data.IsActive
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateDriverDto m)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.DriverId = id;
            return View(m);
        }
        var r = await _driver.UpdateAsync(id, m);
        if (!r.Success)
        {
            ModelState.AddModelError("", r.Message);
            ViewBag.DriverId = id;
            return View(m);
        }
        TempData["SuccessMessage"] = r.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _driver.DeleteAsync(id);
        TempData["SuccessMessage"] = "Marked inactive.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Toggle(int id)
    {
        var r = await _driver.ToggleActiveAsync(id);
        return Json(new { r.Success, r.Message });
    }

    [HttpGet]
    public async Task<IActionResult> SearchBuses(string? q)
    {
        var r = await _bus.GetDropdownAsync(q);
        return Json(r.Data);
    }

    [HttpPost]
    public async Task<IActionResult> AssignBus([FromBody] AssignBusToDriverDto dto)
    {
        var r = await _driver.AssignBusAsync(dto);
        return Json(new { r.Success, r.Message });
    }
}

[Area("BusCoordinator"), Authorize(Roles = "BusCoordinator")]
public class RouteController : Controller
{
    private readonly IRouteService _route;
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    public RouteController(IRouteService r) => _route = r;

    public async Task<IActionResult> Index(int page = 1, string? search = null)
    {
        ViewBag.Search = search;
        var r = await _route.GetAllAsync(page, 10, search);
        return View(r.Data);
    }

    public async Task<IActionResult> Details(int id)
    {
        var r = await _route.GetByIdAsync(id);
        return r.Success ? View(r.Data) : NotFound();
    }

    [HttpGet] public IActionResult Create() => View();
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRouteDto m)
    {
        if (!ModelState.IsValid)
            return View(m);
        var r = await _route.CreateAsync(m, UserId);
        if (!r.Success)
        {
            ModelState.AddModelError("", r.Message);
            return View(m);
        }
        TempData["SuccessMessage"] = r.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var r = await _route.GetByIdAsync(id);
        if (!r.Success)
            return NotFound(); ViewBag.RouteId = id;
        return View(new UpdateRouteDto
        {
            RouteName = r.Data!.RouteName,
            RouteCode = r.Data.RouteCode,
            MorningTime = r.Data.MorningTime,
            EveningTime = r.Data.EveningTime,
            Description = r.Data.Description
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateRouteDto m)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.RouteId = id;
            return View(m);
        }
        var r = await _route.UpdateAsync(id, m);
        if (!r.Success)
        {
            ModelState.AddModelError("", r.Message);
            ViewBag.RouteId = id; return View(m);
        }
        TempData["SuccessMessage"] = r.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _route.DeleteAsync(id);
        TempData["SuccessMessage"] = "Deleted.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> AddStop([FromBody] CreateStopDto dto)
    {
        var r = await _route.AddStopAsync(dto);
        return Json(new { r.Success, r.Message });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteStop(int stopId)
    {
        var r = await _route.DeleteStopAsync(stopId);
        return Json(new { r.Success, r.Message });
    }
}

[Area("BusCoordinator"), Authorize(Roles = "BusCoordinator")]
public class StudentController : Controller
{
    private readonly IStudentService _s;
    private readonly IBusService _bus;
    private readonly IRouteService _route;
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    public StudentController(IStudentService s, IBusService bus, IRouteService route)
    {
        _s = s;
        _bus = bus;
        _route = route;
    }

    public async Task<IActionResult> Index(int page = 1, string? search = null, string? status = "Active")
    {
        ViewBag.Search = search;
        ViewBag.Status = status;
        var r = await _s.GetAllAsync(page, 10, search, status);
        return View(r.Data);
    }
    [HttpGet] public IActionResult Create() => View();
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateStudentDto m)
    {
        if (!ModelState.IsValid)
            return View(m);
        var r = await _s.CreateAsync(m, UserId);
        if (!r.Success)
        {
            ModelState.AddModelError("", r.Message);
            return View(m);
        }
        TempData["SuccessMessage"] = r.Message;
        return RedirectToAction(nameof(Index));
    }
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var r = await _s.GetByIdAsync(id);
        if (!r.Success)
            return NotFound();
        ViewBag.StudentId = id;
        return View(new UpdateStudentDto
        {
            FullName = r.Data!.FullName,
            PhoneNumber = r.Data.PhoneNumber,
            StudentCode = r.Data.StudentCode,
            Standard = r.Data.Standard,
            BusId = r.Data.BusId,
            StopId = r.Data.StopId,
            IsActive = r.Data.IsActive
        });
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateStudentDto m)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.StudentId = id;
            return View(m);
        }
        var r = await _s.UpdateAsync(id, m);
        if (!r.Success)
        {
            ModelState.AddModelError("", r.Message);
            ViewBag.StudentId = id;
            return View(m);
        }
        TempData["SuccessMessage"] = r.Message;
        return RedirectToAction(nameof(Index));
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _s.DeleteAsync(id);
        TempData["SuccessMessage"] = "Marked inactive.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Toggle(int id)
    {
        var r = await _s.ToggleActiveAsync(id);
        return Json(new { r.Success, r.Message });
    }

    [HttpPost]
    public async Task<IActionResult> AssignBus([FromBody] BusTracking.Common.DTOs.Assign.AssignBusToStudentDto dto)
    {
        var r = await _s.AssignBusAsync(dto);
        return Json(new { r.Success, r.Message });
    }

    // Autocomplete endpoints (mirrors SuperAdmin for views)
    [HttpGet]
    public async Task<IActionResult> SearchBuses(string? q)
    {
        var r = await _bus.GetDropdownAsync(q);
        return Json(r.Data);
    }

    [HttpGet]
    public async Task<IActionResult> Search(string? q)
    {
        var r = await _s.SearchAsync(q);
        return Json(r.Data);
    }

    [HttpGet]
    public async Task<IActionResult> SearchStops(int busId)
    {
        var r = await _route.GetStopsByBusAsync(busId);
        if (!r.Success) return Json(Array.Empty<object>());
        var list = (r.Data ?? [])
            .OrderBy(s => s.StopOrder)
            .Select(s => new
            {
                stopId = s.StopId,
                stopName = s.StopName,
                stopOrder = s.StopOrder,
                morningTime = s.MorningTime,
                eveningTime = s.EveningTime
            });
        return Json(list);
    }

    public async Task<IActionResult> Availability(int studentId)
    {
        var r = await _s.GetAvailabilitiesAsync(studentId);
        ViewBag.StudentId = studentId;
        return View(r.Data);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetAvailability(CreateAvailabilityDto m)
    {
        var r = await _s.SetAvailabilityAsync(m, UserId);
        TempData[r.Success ? "SuccessMessage" : "ErrorMessage"] = r.Message;
        return RedirectToAction(nameof(Availability), new { studentId = m.StudentId });
    }
}

[Area("BusCoordinator"), Authorize(Roles = "BusCoordinator")]
public class FeedbackController : Controller
{
    private readonly IFeedbackService _fb;
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    public FeedbackController(IFeedbackService fb) => _fb = fb;

    public async Task<IActionResult> Index(int page = 1, string? status = null)
    {
        ViewBag.Status = status;
        var r = await _fb.GetAllAsync(page, 10, status);
        return View(r.Data);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var r = await _fb.UpdateStatusAsync(id, status, UserId);
        TempData[r.Success ? "SuccessMessage" : "ErrorMessage"] = r.Message;
        return RedirectToAction(nameof(Index));
    }
}

[Area("BusCoordinator"), Authorize(Roles = "BusCoordinator")]
public class ParentController : Controller
{
    private readonly IParentService _parent;
    private readonly IStudentService _student;
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    public ParentController(IParentService parent, IStudentService student)
    {
        _parent = parent;
        _student = student;
    }

    public async Task<IActionResult> Index(int page = 1, string? search = null, string? status = "Active")
    {
        ViewBag.Search = search;
        ViewBag.Status = status;
        var r = await _parent.GetAllAsync(page, 10, search, status);
        return View(r.Data);
    }
    public async Task<IActionResult> Details(int id)
    {
        var r = await _parent.GetByIdAsync(id);
        return r.Success ? View(r.Data) : NotFound();
    }

    [HttpGet] public IActionResult Create() => View();
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateParentDto m)
    {
        if (!ModelState.IsValid)
            return View(m);
        var r = await _parent.CreateAsync(m, UserId);
        if (!r.Success)
        {
            ModelState.AddModelError("", r.Message);
            return View(m);
        }
        TempData["CreatedUser"] = System.Text.Json.JsonSerializer.Serialize(r.Data);
        TempData["SuccessMessage"] = "Parent created."; return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var r = await _parent.GetByIdAsync(id);
        if (!r.Success)
            return NotFound(); ViewBag.ParentId = id;
        return View(new UpdateParentDto
        {
            FullName = r.Data!.FullName,
            PhoneNumber = r.Data.PhoneNumber,
            IsActive = r.Data.IsActive,
            StudentCodes = r.Data.Students.Select(s => s.StudentCode).ToList()
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateParentDto m)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ParentId = id;
            return View(m);
        }
        var r = await _parent.UpdateAsync(id, m);
        if (!r.Success)
        {
            ModelState.AddModelError("", r.Message);
            ViewBag.ParentId = id;
            return View(m);
        }
        TempData["SuccessMessage"] = r.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _parent.DeleteAsync(id);
        TempData["SuccessMessage"] = "Marked inactive.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Toggle(int id)
    {
        var r = await _parent.ToggleActiveAsync(id);
        return Json(new { r.Success, r.Message });
    }

    [HttpGet]
    public async Task<IActionResult> SearchStudents(string? q)
    {
        var r = await _student.SearchAsync(q);
        return Json(r.Data);
    }
}

[Area("BusCoordinator"), Authorize(Roles = "BusCoordinator")]
public class NotificationController : Controller
{
    private readonly INotificationService _notif;
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    public NotificationController(INotificationService n) => _notif = n;

    public async Task<IActionResult> Index()
    {
        var r = await _notif.GetUserNotificationsAsync(UserId);
        return View(r.Data);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(int id)
    {
        await _notif.MarkAsReadAsync(id, UserId);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead()
    {
        await _notif.MarkAllAsReadAsync(UserId);
        return RedirectToAction(nameof(Index));
    }
}