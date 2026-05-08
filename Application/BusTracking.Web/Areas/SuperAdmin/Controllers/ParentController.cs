using BusTracking.Common.DTOs.Parent;
using BusTracking.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BusTracking.Web.Areas.SuperAdmin.Controllers
{
    public class ParentController : SuperAdminBaseController
    {
        private readonly IParentService _parent;
        public ParentController(IParentService p) => _parent = p;

        public async Task<IActionResult> Index(int page = 1, string? search = null)
        {
            ViewBag.Search = search;
            return View(await _parent.GetAllAsync(page, 10, search).Then());
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
            if (!ModelState.IsValid) return View(m);
            var r = await _parent.CreateAsync(m, CurrentUserId);
            if (!r.Success) { ModelState.AddModelError("", r.Message); return View(m); }
            TempData["SuccessMessage"] = r.Message; return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var r = await _parent.GetByIdAsync(id); if (!r.Success) return NotFound();
            ViewBag.ParentId = id;
            return View(new UpdateParentDto { FullName = r.Data!.FullName, PhoneNumber = r.Data.PhoneNumber });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateParentDto m)
        {
            if (!ModelState.IsValid) { ViewBag.ParentId = id; return View(m); }
            var r = await _parent.UpdateAsync(id, m);
            if (!r.Success) { ModelState.AddModelError("", r.Message); ViewBag.ParentId = id; return View(m); }
            TempData["SuccessMessage"] = r.Message; return RedirectToAction(nameof(Index));
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
