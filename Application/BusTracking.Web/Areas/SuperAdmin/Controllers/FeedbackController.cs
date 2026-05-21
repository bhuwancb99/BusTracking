namespace BusTracking.Web.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin"), Authorize(Roles = "SuperAdmin")]
    public class FeedbackController : Controller
    {
        private readonly IFeedbackService _fb;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        public FeedbackController(IFeedbackService fb) => _fb = fb;

        public async Task<IActionResult> Index(int page = 1, string? status = null)
        {
            ViewBag.Status = status;
            return View(await _fb.GetAllAsync(page, 10, status).D());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var r = await _fb.UpdateStatusAsync(id, status, UserId);
            TempData[r.Success ? "SuccessMessage" : "ErrorMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
