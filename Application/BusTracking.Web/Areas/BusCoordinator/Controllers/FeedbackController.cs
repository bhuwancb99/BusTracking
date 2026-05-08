using BusTracking.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BusTracking.Web.Areas.BusCoordinator.Controllers
{
    public class FeedbackController : CoordBaseController
    {
        private readonly IFeedbackService _fb;

        public FeedbackController(IFeedbackService fb) => _fb = fb;

        public async Task<IActionResult> Index(int page = 1, string? status = null)
        {
            ViewBag.Status = status;
            return View(await _fb.GetAllAsync(page, 10, status).Then());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var r = await _fb.UpdateStatusAsync(id, status, CurrentUserId);
            TempData[r.Success ? "SuccessMessage" : "ErrorMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
