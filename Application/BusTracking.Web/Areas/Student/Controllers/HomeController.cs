namespace BusTracking.Web.Areas.Student.Controllers
{
    [Area("Student"), Authorize(Roles = "Student")]
    public class HomeController : Controller
    {
        private readonly IStudentService _s;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        public HomeController(IStudentService s) => _s = s;

        public async Task<IActionResult> Availability()
        {
            var r = await _s.GetAvailabilitiesAsync(UserId);
            ViewBag.StudentId = UserId;
            return View(r.Data);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SetAvailability(CreateAvailabilityDto m)
        {
            var r = await _s.SetAvailabilityAsync(m, UserId);
            TempData[r.Success ? "SuccessMessage" : "ErrorMessage"] = r.Message;
            return RedirectToAction(nameof(Availability));
        }
    }
}
