namespace BusTracking.Web.Areas.Parent.Controllers
{
    [Area("Parent"), Authorize(Roles = "Parent")]
    public class FeedbackController : Controller
    {
        private readonly IFeedbackService _fb;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        public FeedbackController(IFeedbackService fb) => _fb = fb;

        [HttpGet] public IActionResult Submit() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(CreateFeedbackDto m)
        {
            if (!ModelState.IsValid)
                return View(m);
            var r = await _fb.CreateAsync(m, UserId);
            if (!r.Success)
            {
                ModelState.AddModelError("", r.Message);
                return View(m);
            }
            TempData["SuccessMessage"] = r.Message; return RedirectToAction(nameof(Submit));
        }
    }
}
