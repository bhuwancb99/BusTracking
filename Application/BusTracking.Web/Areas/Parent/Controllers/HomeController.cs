using BusTracking.Common.DTOs.Availability;
using BusTracking.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BusTracking.Web.Areas.Parent.Controllers
{
    public class HomeController : ParentBaseController
    {
        private readonly IStudentService _student;
        private readonly ITripService _trip;
        public HomeController(IStudentService s, ITripService t) { _student = s; _trip = t; }

        // My children's availability management
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
