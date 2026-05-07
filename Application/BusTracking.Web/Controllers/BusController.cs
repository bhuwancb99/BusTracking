using BusTracking.Common.DTOs.Bus;
using BusTracking.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusTracking.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin,BusCoordinator")]
    public class BusController : BaseController
    {
        private readonly IBusService _bus;
        public BusController(IBusService bus) => _bus = bus;

        public async Task<IActionResult> Index(int page = 1, string? search = null)
        {
            var result = await _bus.GetAllAsync(page, 10, search);
            ViewBag.Search = search;
            return View(result.Data);
        }

        public async Task<IActionResult> Details(int id)
        {
            var result = await _bus.GetByIdAsync(id);
            if (!result.Success) return NotFound();
            return View(result.Data);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBusDto model)
        {
            if (!ModelState.IsValid) return View(model);
            var result = await _bus.CreateAsync(model, CurrentUserId);
            if (!result.Success) { ModelState.AddModelError("", result.Message); return View(model); }
            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _bus.GetByIdAsync(id);
            if (!result.Success) return NotFound();
            var edit = new UpdateBusDto
            {
                BusName = result.Data!.BusName,
                BusNumber = result.Data.BusNumber,
                RouteId = result.Data.RouteId,
                Capacity = result.Data.Capacity
            };
            ViewBag.BusId = id;
            return View(edit);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateBusDto model)
        {
            if (!ModelState.IsValid) { ViewBag.BusId = id; return View(model); }
            var result = await _bus.UpdateAsync(id, model);
            if (!result.Success) { ModelState.AddModelError("", result.Message); ViewBag.BusId = id; return View(model); }
            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _bus.DeleteAsync(id);
            TempData["SuccessMessage"] = "Bus deleted.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AssignStudent(int busId, int studentId)
        {
            var r = await _bus.AssignStudentAsync(busId, studentId);
            return Json(new { success = r.Success, message = r.Message });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveStudent(int busId, int studentId)
        {
            var r = await _bus.RemoveStudentAsync(busId, studentId);
            return Json(new { success = r.Success, message = r.Message });
        }
    }
}
