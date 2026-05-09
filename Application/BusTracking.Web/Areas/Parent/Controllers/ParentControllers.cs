using BusTracking.Common.DTOs.Availability;
using BusTracking.Common.DTOs.Feedback;
using BusTracking.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BusTracking.Web.Areas.Parent.Controllers;

[Area("Parent"), Authorize(Roles = "Parent")]
public class DashboardController : Controller
{
    public IActionResult Index() => View();
}

[Area("Parent"), Authorize(Roles = "Parent")]
public class HomeController : Controller
{
    private readonly IStudentService _s;
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    public HomeController(IStudentService s) => _s = s;
    public async Task<IActionResult> Availability(int studentId = 0)
    { var id = studentId > 0 ? studentId : UserId; var r = await _s.GetAvailabilitiesAsync(id); ViewBag.StudentId = id; return View(r.Data); }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetAvailability(CreateAvailabilityDto m)
    {
        var r = await _s.SetAvailabilityAsync(m, UserId);
        TempData[r.Success ? "SuccessMessage" : "ErrorMessage"] = r.Message;
        return RedirectToAction(nameof(Availability), new { studentId = m.StudentId });
    }
}

[Area("Parent"), Authorize(Roles = "Parent")]
public class TrackingController : Controller
{
    private readonly ITripService _trip;
    public TrackingController(ITripService t) => _trip = t;
    public async Task<IActionResult> Track(int tripId = 0)
    {
        var s = await _trip.GetTripStudentsAsync(tripId); var l = await _trip.GetLatestLocationAsync(tripId);
        ViewBag.TripId = tripId; ViewBag.Location = l.Data; return View(s.Data);
    }
}

[Area("Parent"), Authorize(Roles = "Parent")]
public class FeedbackController : Controller
{
    private readonly IFeedbackService _fb;
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    public FeedbackController(IFeedbackService fb) => _fb = fb;
    [HttpGet] public IActionResult Submit() => View();
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(CreateFeedbackDto m)
    {
        if (!ModelState.IsValid) return View(m); var r = await _fb.CreateAsync(m, UserId);
        if (!r.Success) { ModelState.AddModelError("", r.Message); return View(m); }
        TempData["SuccessMessage"] = r.Message; return RedirectToAction(nameof(Submit));
    }
}
