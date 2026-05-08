using BusTracking.Common.DTOs.Student;
using BusTracking.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BusTracking.Web.Areas.BusCoordinator.Controllers
{
    public class StudentController : CoordBaseController
    {
        private readonly IStudentService _s;

        public StudentController(IStudentService s) => _s = s;

        public async Task<IActionResult> Index(int page = 1, string? search = null)
        {
            ViewBag.Search = search;
            return View(await _s.GetAllAsync(page, 10, search).Then());
        }

        [HttpGet] public IActionResult Create() => View();
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateStudentDto m)
        {
            if (!ModelState.IsValid) return View(m);
            var r = await _s.CreateAsync(m, CurrentUserId);
            if (!r.Success) { ModelState.AddModelError("", r.Message); return View(m); }
            TempData["SuccessMessage"] = r.Message; return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var r = await _s.GetByIdAsync(id); if (!r.Success) return NotFound();
            ViewBag.StudentId = id;
            return View(new UpdateStudentDto { FullName = r.Data!.FullName, Standard = r.Data.Standard });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateStudentDto m)
        {
            if (!ModelState.IsValid) { ViewBag.StudentId = id; return View(m); }
            var r = await _s.UpdateAsync(id, m);
            if (!r.Success) { ModelState.AddModelError("", r.Message); ViewBag.StudentId = id; return View(m); }
            TempData["SuccessMessage"] = r.Message; return RedirectToAction(nameof(Index));
        }
    }
}
