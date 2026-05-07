using BusTracking.Common.DTOs.Route;
using BusTracking.Common.DTOs.Stop;
using BusTracking.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusTracking.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin,BusCoordinator")]
    public class RouteController : BaseController
    {
        private readonly IRouteService _route;
        public RouteController(IRouteService route) => _route = route;

        public async Task<IActionResult> Index(int page = 1, string? search = null)
        {
            var r = await _route.GetAllAsync(page, 10, search);
            ViewBag.Search = search;
            return View(r.Data);
        }

        public async Task<IActionResult> Details(int id)
        {
            var r = await _route.GetByIdAsync(id);
            if (!r.Success) return NotFound();
            return View(r.Data);
        }

        [HttpGet] public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRouteDto model)
        {
            if (!ModelState.IsValid) return View(model);
            var r = await _route.CreateAsync(model, CurrentUserId);
            if (!r.Success) { ModelState.AddModelError("", r.Message); return View(model); }
            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var r = await _route.GetByIdAsync(id);
            if (!r.Success) return NotFound();
            ViewBag.RouteId = id;
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
        public async Task<IActionResult> Edit(int id, UpdateRouteDto model)
        {
            if (!ModelState.IsValid) { ViewBag.RouteId = id; return View(model); }
            var r = await _route.UpdateAsync(id, model);
            if (!r.Success) { ModelState.AddModelError("", r.Message); ViewBag.RouteId = id; return View(model); }
            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _route.DeleteAsync(id);
            TempData["SuccessMessage"] = "Route deleted.";
            return RedirectToAction(nameof(Index));
        }

        // ── Stops (AJAX-friendly actions) ────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> AddStop([FromBody] CreateStopDto dto)
        {
            var r = await _route.AddStopAsync(dto);
            return Json(new { success = r.Success, message = r.Message });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStop(int stopId)
        {
            var r = await _route.DeleteStopAsync(stopId);
            return Json(new { success = r.Success, message = r.Message });
        }
    }
}
