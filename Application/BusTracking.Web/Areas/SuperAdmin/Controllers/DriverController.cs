using BusTracking.Common.DTOs.Driver;
using BusTracking.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BusTracking.Web.Areas.SuperAdmin.Controllers
{
    public class DriverController : SuperAdminBaseController
    {
        private readonly IDriverService _driver;
        public DriverController(IDriverService driver) => _driver = driver;

        public async Task<IActionResult> Index(int page = 1, string? search = null)
        {
            ViewBag.Search = search;
            return View(await _driver.GetAllAsync(page, 10, search).Then());
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
            if (!ModelState.IsValid) return View(m);
            var r = await _driver.CreateAsync(m, CurrentUserId);
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
        {
            await _driver.DeleteAsync(id);
            TempData["SuccessMessage"] = "Driver deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
