using BusTracking.Common.DTOs.Trip;
using BusTracking.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BusTracking.Web.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin"), Authorize(Roles = "SuperAdmin")]
    public class TripController : Controller
    {
        private readonly ITripService _trip;
        private readonly IBusService _bus;
        private readonly IDriverService _driver;
        private readonly IRouteService _route;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        public TripController(ITripService trip, IBusService bus, IDriverService driver, IRouteService route)
        {
            _trip = trip;
            _bus = bus;
            _driver = driver;
            _route = route;
        }

        // ── Index: list all trips ────────────────────────────────────
        public async Task<IActionResult> Index(int page = 1, string? busId = null, string? status = null)
        {
            ViewBag.BusId = busId;
            ViewBag.Status = status;
            var r = await _trip.GetAllAsync(page, 15, busId);
            // filter by status client-side via ViewBag
            return View(r.Data);
        }

        // ── Create form ──────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadDropdowns();
            return View(new CreateTripDto
            {
                TripDate = DateTime.Today.ToString("yyyy-MM-dd"),
                TripType = "Morning"
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTripDto m)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdowns();
                return View(m);
            }
            var r = await _trip.CreateAsync(m, UserId);
            if (!r.Success)
            {
                ModelState.AddModelError("", r.Message);
                await LoadDropdowns();
                return View(m);
            }
            TempData["SuccessMessage"] = $"Trip created for {r.Data?.TripDate} ({r.Data?.TripType}). Students and stops auto-added.";
            return RedirectToAction(nameof(Details), new { id = r.Data?.TripId });
        }

        // ── Details: trip overview + students + stops ────────────────
        public async Task<IActionResult> Details(int id)
        {
            var tripR = await _trip.GetByIdAsync(id);
            if (!tripR.Success)
                return NotFound();
            var students = await _trip.GetTripStudentsAsync(id);
            var stops = await _trip.GetStopEventsAsync(id);
            ViewBag.Students = students.Data ?? [];
            ViewBag.Stops = stops.Data ?? [];
            return View(tripR.Data);
        }

        // ── Start trip ───────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Start(int id)
        {
            var r = await _trip.StartTripAsync(id);
            TempData[r.Success ? "SuccessMessage" : "ErrorMessage"] = r.Message;
            return RedirectToAction(nameof(Details), new { id });
        }

        // ── End trip ─────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> End(int id)
        {
            var r = await _trip.EndTripAsync(id);
            TempData[r.Success ? "SuccessMessage" : "ErrorMessage"] = r.Message;
            return RedirectToAction(nameof(Details), new { id });
        }

        // ── Cancel trip ──────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var r = await _trip.CancelTripAsync(id);
            TempData[r.Success ? "SuccessMessage" : "ErrorMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        // ── Delete trip ──────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var r = await _trip.DeleteAsync(id);
            TempData[r.Success ? "SuccessMessage" : "ErrorMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        // ── Mark stop reached (AJAX) ─────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> ReachStop(int tripId, int stopId)
        {
            var r = await _trip.ReachStopAsync(tripId, stopId);
            return Json(new { r.Success, r.Message });
        }

        // ── Update student boarding (AJAX) ───────────────────────────
        [HttpPost]
        public async Task<IActionResult> UpdateBoarding([FromBody] UpdateBoardingRequest req)
        {
            var r = await _trip.UpdateBoardingAsync(req.TripId, req.StudentId, req.StopId, req.Status);
            return Json(new { r.Success, r.Message });
        }

        // ── Simulate GPS ping (AJAX) — for testing without MAUI ──────
        [HttpPost]
        public async Task<IActionResult> SimulateGps([FromBody] LocationPingDto dto)
        {
            var r = await _trip.InsertLocationPingAsync(
                dto.TripId, dto.BusId, dto.Latitude, dto.Longitude, dto.Speed, dto.Heading);
            return Json(new { r.Success, r.Message });
        }

        // ── Get latest location (AJAX) ───────────────────────────────
        [HttpGet]
        public async Task<IActionResult> LatestLocation(int tripId)
        {
            var r = await _trip.GetLatestLocationAsync(tripId);
            return Json(new { success = r.Success, data = r.Data });
        }

        // ── Get stops for bus route (AJAX) ───────────────────────────
        [HttpGet]
        public async Task<IActionResult> StopsForBus(int busId)
        {
            var r = await _route.GetStopsByBusAsync(busId);
            return Json(r.Data ?? []);
        }

        // ── Get drivers for bus (AJAX) ───────────────────────────────
        [HttpGet]
        public async Task<IActionResult> SearchDrivers(string? q)
        {
            var r = await _driver.GetDropdownAsync(q);
            return Json(r.Data ?? []);
        }

        // ── Dropdown loader ──────────────────────────────────────────
        private async Task LoadDropdowns()
        {
            var buses = await _bus.GetAllAsync(1, 100, null, "Active");
            var drivers = await _driver.GetAllAsync(1, 100, null, "Active");
            var routes = await _route.GetAllAsync(1, 100, null);

            ViewBag.Buses = (buses.Data?.Items ?? [])
                .Select(b => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                { Value = b.BusId.ToString(), Text = $"{b.BusName} ({b.BusNumber})" }).ToList();

            ViewBag.Drivers = (drivers.Data?.Items ?? [])
                .Select(d => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                { Value = d.UserId.ToString(), Text = d.FullName }).ToList();

            ViewBag.Routes = (routes.Data?.Items ?? [])
                .Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                { Value = r.RouteId.ToString(), Text = $"{r.RouteName} ({r.RouteCode})" }).ToList();
        }
    }
}
