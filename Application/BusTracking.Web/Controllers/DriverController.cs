using BusTracking.Common.DTOs.Driver;
using BusTracking.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusTracking.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin,BusCoordinator")]
    public class DriverController : BaseController
    {
        private readonly IDriverService _driver;
        public DriverController(IDriverService driver) => _driver = driver;

        public async Task<IActionResult> Index(int page = 1, string? search = null)
        {
            var r = await _driver.GetAllAsync(page, 10, search);
            ViewBag.Search = search;
            return View(r.Data);
        }

        public async Task<IActionResult> Details(int id)
        {
            var r = await _driver.GetByIdAsync(id);
            if (!r.Success) return NotFound();
            return View(r.Data);
        }

        [HttpGet] public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDriverDto model)
        {
            if (!ModelState.IsValid) return View(model);
            var r = await _driver.CreateAsync(model, CurrentUserId);
            if (!r.Success) { ModelState.AddModelError("", r.Message); return View(model); }
            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var r = await _driver.GetByIdAsync(id);
            if (!r.Success) return NotFound();
            ViewBag.DriverId = id;
            return View(new UpdateDriverDto
            {
                FullName = r.Data!.FullName,
                PhoneNumber = r.Data.PhoneNumber,
                LicenseNumber = r.Data.LicenseNumber
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateDriverDto model)
        {
            if (!ModelState.IsValid) { ViewBag.DriverId = id; return View(model); }
            var r = await _driver.UpdateAsync(id, model);
            if (!r.Success) { ModelState.AddModelError("", r.Message); ViewBag.DriverId = id; return View(model); }
            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _driver.DeleteAsync(id);
            TempData["SuccessMessage"] = "Driver deleted.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AssignBus(int driverUserId, int busId)
        {
            var r = await _driver.AssignBusAsync(driverUserId, busId);
            return Json(new { success = r.Success, message = r.Message });
        }
    }
}
