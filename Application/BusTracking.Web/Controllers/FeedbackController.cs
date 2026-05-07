using BusTracking.Common.DTOs.Feedback;
using BusTracking.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusTracking.Web.Controllers
{
    [Authorize]
    public class FeedbackController : BaseController
    {
        private readonly IFeedbackService _feedback;
        public FeedbackController(IFeedbackService feedback) => _feedback = feedback;

        // Admin/Coordinator: see all feedback
        [Authorize(Roles = "SuperAdmin,BusCoordinator")]
        public async Task<IActionResult> Index(int page = 1, string? status = null)
        {
            var r = await _feedback.GetAllAsync(page, 10, status);
            ViewBag.Status = status;
            return View(r.Data);
        }

        // All users: submit feedback
        [HttpGet]
        public IActionResult Submit() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(CreateFeedbackDto model)
        {
            if (!ModelState.IsValid) return View(model);
            var r = await _feedback.CreateAsync(model, CurrentUserId);
            if (!r.Success) { ModelState.AddModelError("", r.Message); return View(model); }
            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Submit));
        }

        // Admin: update feedback status
        [Authorize(Roles = "SuperAdmin,BusCoordinator")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var r = await _feedback.UpdateStatusAsync(id, status, CurrentUserId);
            TempData[r.Success ? "SuccessMessage" : "ErrorMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
