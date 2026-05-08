using BusTracking.Common.DTOs.Feedback;
using BusTracking.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BusTracking.Web.Areas.Student.Controllers
{
    public class FeedbackController : StudentBaseController
    {
        private readonly IFeedbackService _fb;
        public FeedbackController(IFeedbackService fb) => _fb = fb;

        [HttpGet] public IActionResult Submit() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(CreateFeedbackDto m)
        {
            if (!ModelState.IsValid) return View(m);
            var r = await _fb.CreateAsync(m, CurrentUserId);
            if (!r.Success) { ModelState.AddModelError("", r.Message); return View(m); }
            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Submit));
        }
    }
}
