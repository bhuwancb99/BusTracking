using BusTracking.Common.DTOs.Availability;
using BusTracking.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BusTracking.Web.Areas.Student.Controllers
{
    public class HomeController : StudentBaseController
    {
        private readonly IStudentService _student;
        public HomeController(IStudentService s) => _student = s;

        public async Task<IActionResult> Availability()
        {
            // studentId resolved from logged in user's linked StudentDetail
            var r = await _student.GetAvailabilitiesAsync(CurrentUserId);  // service resolves by userId
            ViewBag.StudentId = CurrentUserId;
            return View(r.Data);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SetAvailability(CreateAvailabilityDto m)
        {
            var r = await _student.SetAvailabilityAsync(m, CurrentUserId);
            TempData[r.Success ? "SuccessMessage" : "ErrorMessage"] = r.Message;
            return RedirectToAction(nameof(Availability));
        }
    }
}
