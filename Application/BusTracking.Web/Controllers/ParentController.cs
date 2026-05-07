using BusTracking.Common.DTOs.Parent;
using BusTracking.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusTracking.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin,BusCoordinator")]
    public class ParentController : BaseController
    {
        private readonly IParentService _parent;
        public ParentController(IParentService parent) => _parent = parent;

        public async Task<IActionResult> Index(int page = 1, string? search = null)
        {
            var r = await _parent.GetAllAsync(page, 10, search);
            ViewBag.Search = search;
            return View(r.Data);
        }

        public async Task<IActionResult> Details(int id)
        {
            var r = await _parent.GetByIdAsync(id);
            if (!r.Success) return NotFound();
            return View(r.Data);
        }

        [HttpGet] public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateParentDto model)
        {
            if (!ModelState.IsValid) return View(model);
            var r = await _parent.CreateAsync(model, CurrentUserId);
            if (!r.Success) { ModelState.AddModelError("", r.Message); return View(model); }
            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var r = await _parent.GetByIdAsync(id);
            if (!r.Success) return NotFound();
            ViewBag.ParentId = id;
            return View(new UpdateParentDto
            {
                FullName = r.Data!.FullName,
                PhoneNumber = r.Data.PhoneNumber,
                StudentCodes = []
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateParentDto model)
        {
            if (!ModelState.IsValid) { ViewBag.ParentId = id; return View(model); }
            var r = await _parent.UpdateAsync(id, model);
            if (!r.Success) { ModelState.AddModelError("", r.Message); ViewBag.ParentId = id; return View(model); }
            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _parent.DeleteAsync(id);
            TempData["SuccessMessage"] = "Parent deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
