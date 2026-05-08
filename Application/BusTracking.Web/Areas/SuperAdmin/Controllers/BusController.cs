using BusTracking.Common.DTOs.Bus;
using BusTracking.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BusTracking.Web.Areas.SuperAdmin.Controllers
{
    public class BusController : SuperAdminBaseController
    {
        private readonly IBusService _bus;
        public BusController(IBusService bus) => _bus = bus;

        public async Task<IActionResult> Index(int page = 1, string? search = null)
        {
            ViewBag.Search = search;
            return View(await _bus.GetAllAsync(page, 10, search).Then());
        }

        public async Task<IActionResult> Details(int id)
        {
            var r = await _bus.GetByIdAsync(id);
            return r.Success ? View(r.Data) : NotFound();
        }

        [HttpGet] public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBusDto m)
        {
            if (!ModelState.IsValid) return View(m);
            var r = await _bus.CreateAsync(m, CurrentUserId);
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
        {
            await _bus.DeleteAsync(id);
            TempData["SuccessMessage"] = "Bus deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
