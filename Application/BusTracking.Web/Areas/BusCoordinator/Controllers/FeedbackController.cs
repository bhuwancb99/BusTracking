namespace BusTracking.Web.Areas.BusCoordinator.Controllers
{
    [Area("BusCoordinator"), Authorize(Roles = "BusCoordinator")]
    public class FeedbackController : Controller
    {
        private readonly IFeedbackService _fb;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        public FeedbackController(IFeedbackService fb) => _fb = fb;

        public async Task<IActionResult> Index(int page = 1, string? status = null)
        {
            if (!PermissionHelper.Can(User, "helpsupport.view") && !PermissionHelper.Can(User, "helpsupport.manage")) return Forbid();
            ViewBag.Status = status;
            var r = await _fb.GetAllAsync(page, 10, status);
            return View(r.Data);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            if (!PermissionHelper.Can(User, "helpsupport.manage")) return Forbid();
            var r = await _fb.UpdateStatusAsync(id, status, UserId);
            TempData[r.Success ? "SuccessMessage" : "ErrorMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
