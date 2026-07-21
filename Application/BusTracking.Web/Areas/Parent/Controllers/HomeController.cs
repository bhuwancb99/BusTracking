namespace BusTracking.Web.Areas.Parent.Controllers
{
    [Area("Parent"), Authorize(Roles = "Parent")]
    public class HomeController : Controller
    {
        private readonly IStudentService _s;
        private readonly IParentService _parent;
        private readonly AppDbContext _db;
        private readonly IImageService _img;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        public HomeController(IStudentService s, IParentService p, AppDbContext db, IImageService img)
        {
            _s = s;
            _parent = p;
            _db = db;
            _img = img;
        }

        public async Task<IActionResult> Availability(int studentId = 0)
        {
            var parentRes = await _parent.GetByIdAsync(UserId);
            var studentIds = parentRes.Success && parentRes.Data != null
                ? parentRes.Data.Students.Select(s => s.StudentId).ToList()
                : new List<int>();

            var children = await _db.Students
                .Include(s => s.User)
                .Include(s => s.Standard)
                .Where(s => studentIds.Contains(s.StudentId) && s.User.IsActive)
                .ToListAsync();

            ViewBag.Children = children;

            var selectedStudent = (studentId > 0 ? children.FirstOrDefault(c => c.StudentId == studentId) : null)
                                ?? children.FirstOrDefault();

            ViewBag.SelectedStudent = selectedStudent;
            var targetStudentId = selectedStudent?.StudentId ?? 0;
            ViewBag.StudentId = targetStudentId;

            if (targetStudentId == 0)
            {
                return View(new List<AvailabilityDto>());
            }

            var r = await _s.GetAvailabilitiesAsync(targetStudentId);
            return View(r.Data ?? new List<AvailabilityDto>());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SetAvailability(CreateAvailabilityDto m)
        {
            var r = await _s.SetAvailabilityAsync(m, UserId);
            TempData[r.Success ? "SuccessMessage" : "ErrorMessage"] = r.Message;
            return RedirectToAction(nameof(Availability), new { studentId = m.StudentId });
        }

        // ── GET /Parent/Home/MyChildren ───────────────────────────────
        [HttpGet]
        public async Task<IActionResult> MyChildren()
        {
            var parent = await _parent.GetByIdAsync(UserId);
            if (!parent.Success) return Json(Array.Empty<object>());

            var studentUserIds = parent.Data!.Students.Select(s => s.StudentId).ToList();

            var studentUsers = await _db.Students
                .Include(s => s.User)
                .Include(s => s.Bus)
                .Include(s => s.Stop)
                .Include(s => s.Standard)
                .Where(s => studentUserIds.Contains(s.StudentId))
                .ToListAsync();

            var children = studentUsers.Select(s => new
            {
                s.StudentId,
                s.StudentCode,
                FullName = s.User.FullName,
                StandardName = s.Standard?.StandardName,
                BusNumber = s.Bus?.BusNumber,
                BusName = s.Bus?.BusName,
                StopName = s.Stop?.StopName,
                IsActive = s.User.IsActive,
                ProfileImageUrl = s.User.ProfileImageUrl,
                UserId = s.UserId
            });

            return Json(children);
        }

        // ── POST /Parent/Home/UpdateChildPhoto?studentUserId=88 ───────
        [HttpPost, ValidateAntiForgeryToken]
        [RequestSizeLimit(5_242_880)]
        public async Task<IActionResult> UpdateChildPhoto(int studentUserId, IFormFile file)
        {
            var parent = await _parent.GetByIdAsync(UserId);
            if (!parent.Success)
                return Json(new { success = false, message = "Parent not found." });

            var studentIds = parent.Data!.Students.Select(s => s.StudentId).ToList();
            var student = await _db.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == studentUserId && studentIds.Contains(s.StudentId));

            if (student is null)
                return Json(new { success = false, message = "Student not linked to your account." });

            try
            {
                var url = await _img.SaveProfileImageAsync(
                    file, studentUserId, "student", student.User.ProfileImageUrl);

                student.User.ProfileImageUrl = url;
                student.User.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                return Json(new { success = true, imageUrl = url });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
