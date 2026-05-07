using BusTracking.Common.DTOs.SubAdmin;
using BusTracking.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusTracking.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class SubAdminController : BaseController
    {
        private readonly ISubAdminService _subAdmin;
        public SubAdminController(ISubAdminService subAdmin) => _subAdmin = subAdmin;

        public async Task<IActionResult> Index(int page = 1, string? search = null)
        {
            var r = await _subAdmin.GetAllAsync(page, 10, search);
            ViewBag.Search = search;
            return View(r.Data);
        }

        public async Task<IActionResult> Details(int id)
        {
            var r = await _subAdmin.GetByIdAsync(id);
            if (!r.Success) return NotFound();
            return View(r.Data);
        }

        [HttpGet] public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateSubAdminDto model)
        {
            if (!ModelState.IsValid) return View(model);
            var r = await _subAdmin.CreateAsync(model, CurrentUserId);
            if (!r.Success) { ModelState.AddModelError("", r.Message); return View(model); }
            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var r = await _subAdmin.GetByIdAsync(id);
            if (!r.Success) return NotFound();
            ViewBag.SubAdminId = id;
            return View(new UpdateSubAdminDto
            {
                FullName = r.Data!.FullName,
                PhoneNumber = r.Data.PhoneNumber,
                PermissionIds = []
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateSubAdminDto model)
        {
            if (!ModelState.IsValid) { ViewBag.SubAdminId = id; return View(model); }
            var r = await _subAdmin.UpdateAsync(id, model);
            if (!r.Success) { ModelState.AddModelError("", r.Message); ViewBag.SubAdminId = id; return View(model); }
            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _subAdmin.DeleteAsync(id);
            TempData["SuccessMessage"] = "Coordinator deleted.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var r = await _subAdmin.ToggleActiveAsync(id);
            return Json(new { success = r.Success, message = r.Message });
        }
    }
}
