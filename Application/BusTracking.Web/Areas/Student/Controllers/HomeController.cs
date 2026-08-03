namespace BusTracking.Web.Areas.Student.Controllers
{
    [Area("Student"), Authorize(Roles = "Student")]
    public class HomeController : Controller
    {
        private readonly IStudentService _s;
        private readonly AppDbContext _db;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        public HomeController(IStudentService s, AppDbContext db)
        {
            _s = s;
            _db = db;
        }

        public async Task<IActionResult> Availability()
        {
            var student = await _db.Students.FirstOrDefaultAsync(s => s.UserId == UserId);
            if (student == null)
            {
                ViewBag.StudentId = 0;
                return View(new List<AvailabilityDto>());
            }

            var r = await _s.GetAvailabilitiesAsync(student.StudentId);
            ViewBag.StudentId = student.StudentId;
            return View(r.Data ?? new List<AvailabilityDto>());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SetAvailability(CreateAvailabilityDto m)
        {
            var student = await _db.Students.FirstOrDefaultAsync(s => s.UserId == UserId);
            if (student == null)
            {
                TempData["ErrorMessage"] = "Student profile not found.";
                return RedirectToAction(nameof(Availability));
            }

            m.StudentId = student.StudentId;
            var r = await _s.SetAvailabilityAsync(m, UserId);
            TempData[r.Success ? "SuccessMessage" : "ErrorMessage"] = r.Message;
            return RedirectToAction(nameof(Availability));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAvailability(UpdateAvailabilityDto m)
        {
            var student = await _db.Students.FirstOrDefaultAsync(s => s.UserId == UserId);
            if (student == null)
            {
                TempData["ErrorMessage"] = "Student profile not found.";
                return RedirectToAction(nameof(Availability));
            }

            m.StudentId = student.StudentId;
            var r = await _s.UpdateAvailabilityAsync(m, UserId);
            TempData[r.Success ? "SuccessMessage" : "ErrorMessage"] = r.Message;
            return RedirectToAction(nameof(Availability));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAvailability(int availabilityId)
        {
            var student = await _db.Students.FirstOrDefaultAsync(s => s.UserId == UserId);
            if (student == null)
            {
                TempData["ErrorMessage"] = "Student profile not found.";
                return RedirectToAction(nameof(Availability));
            }

            var r = await _s.DeleteAvailabilityAsync(availabilityId, student.StudentId);
            TempData[r.Success ? "SuccessMessage" : "ErrorMessage"] = r.Message;
            return RedirectToAction(nameof(Availability));
        }
    }
}
