using BusTracking.Common.DTOs.Availability;
using BusTracking.Common.DTOs.Student;
using BusTracking.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BusTracking.Web.Areas.SuperAdmin.Controllers
{
    public class StudentController : SuperAdminBaseController
    {
        private readonly IStudentService _student;
        public StudentController(IStudentService s) => _student = s;

        public async Task<IActionResult> Index(int page = 1, string? search = null)
        {
            ViewBag.Search = search;
            return View(await _student.GetAllAsync(page, 10, search).Then());
        }

        [HttpGet] public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateStudentDto m)
        {
            if (!ModelState.IsValid) return View(m);
            var r = await _student.CreateAsync(m, CurrentUserId);
            if (!r.Success) { ModelState.AddModelError("", r.Message); return View(m); }
            TempData["SuccessMessage"] = r.Message; return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var r = await _student.GetByIdAsync(id); if (!r.Success) return NotFound();
            ViewBag.StudentId = id;
            return View(new UpdateStudentDto { FullName = r.Data!.FullName, Standard = r.Data.Standard });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateStudentDto m)
        {
            if (!ModelState.IsValid) { ViewBag.StudentId = id; return View(m); }
            var r = await _student.UpdateAsync(id, m);
            if (!r.Success) { ModelState.AddModelError("", r.Message); ViewBag.StudentId = id; return View(m); }
            TempData["SuccessMessage"] = r.Message; return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _student.DeleteAsync(id);
            TempData["SuccessMessage"] = "Student deleted.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Availability(int studentId)
        {
            var r = await _student.GetAvailabilitiesAsync(studentId);
            ViewBag.StudentId = studentId;
            return View(r.Data);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SetAvailability(CreateAvailabilityDto m)
        {
            var r = await _student.SetAvailabilityAsync(m, CurrentUserId);
            TempData[r.Success ? "SuccessMessage" : "ErrorMessage"] = r.Message;
            return RedirectToAction(nameof(Availability), new { studentId = m.StudentId });
        }
    }
}
