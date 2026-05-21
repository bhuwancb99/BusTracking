namespace BusTracking.Web.Areas.Parent.Controllers
{
    [Area("Parent"), Authorize(Roles = "Parent")]
    public class HomeController : Controller
    {
        private readonly IStudentService _s;
        private readonly IParentService _parent;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        public HomeController(IStudentService s, IParentService p)
        {
            _s = s;
            _parent = p;
        }

        public async Task<IActionResult> Availability(int studentId = 0)
        {
            var id = studentId > 0 ? studentId : UserId;
            var r = await _s.GetAvailabilitiesAsync(id);
            ViewBag.StudentId = id;
            return View(r.Data);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SetAvailability(CreateAvailabilityDto m)
        {
            var r = await _s.SetAvailabilityAsync(m, UserId);
            TempData[r.Success ? "SuccessMessage" : "ErrorMessage"] = r.Message;
            return RedirectToAction(nameof(Availability), new { studentId = m.StudentId });
        }

        [HttpGet]
        public async Task<IActionResult> MyChildren()
        {
            var parent = await _parent.GetByIdAsync(UserId);
            if (!parent.Success) return Json(Array.Empty<object>());
            var children = parent.Data!.Students.Select(s => new
            {
                s.StudentId,
                s.StudentCode,
                s.FullName,
                s.Standard,
                s.BusNumber,
                StopName = (string?)null,
                IsActive = true
            });
            return Json(children);
        }
    }
}
