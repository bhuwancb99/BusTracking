using BusTracking.Common.DTOs.Availability;
using BusTracking.Common.DTOs.Student;
using BusTracking.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusTracking.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin,BusCoordinator")]
    public class StudentController : BaseController
    {
        private readonly IStudentService _student;
        public StudentController(IStudentService student) => _student = student;

        public async Task<IActionResult> Index(int page = 1, string? search = null)
        {
            var result = await _student.GetAllAsync(page, 10, search);
            ViewBag.Search = search;
            return View(result.Data);
        }

        public async Task<IActionResult> Details(int id)
        {
            var r = await _student.GetByIdAsync(id);
            if (!r.Success) return NotFound();
            return View(r.Data);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateStudentDto model)
        {
            if (!ModelState.IsValid) return View(model);
            var r = await _student.CreateAsync(model, CurrentUserId);
            if (!r.Success) { ModelState.AddModelError("", r.Message); return View(model); }
            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var r = await _student.GetByIdAsync(id);
            if (!r.Success) return NotFound();
            ViewBag.StudentId = id;
            return View(new UpdateStudentDto
            {
                FullName = r.Data!.FullName,
                Standard = r.Data.Standard
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateStudentDto model)
        {
            if (!ModelState.IsValid) { ViewBag.StudentId = id; return View(model); }
            var r = await _student.UpdateAsync(id, model);
            if (!r.Success) { ModelState.AddModelError("", r.Message); ViewBag.StudentId = id; return View(model); }
            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _student.DeleteAsync(id);
            TempData["SuccessMessage"] = "Student deleted.";
            return RedirectToAction(nameof(Index));
        }

        // Student/Parent availability management
        [Authorize(Roles = "SuperAdmin,BusCoordinator,Student,Parent")]
        [HttpGet]
        public async Task<IActionResult> Availability(int studentId)
        {
            var r = await _student.GetAvailabilitiesAsync(studentId);
            ViewBag.StudentId = studentId;
            return View(r.Data);
        }

        [Authorize(Roles = "SuperAdmin,BusCoordinator,Student,Parent")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SetAvailability(CreateAvailabilityDto model)
        {
            var r = await _student.SetAvailabilityAsync(model, CurrentUserId);
            TempData[r.Success ? "SuccessMessage" : "ErrorMessage"] = r.Message;
            return RedirectToAction(nameof(Availability), new { studentId = model.StudentId });
        }
    }
}
