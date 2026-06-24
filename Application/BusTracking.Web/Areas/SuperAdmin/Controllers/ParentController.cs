namespace BusTracking.Web.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin"), Authorize(Roles = "SuperAdmin")]
    public class ParentController : Controller
    {
        private readonly IParentService _parent;
        private readonly IStudentService _student;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        public ParentController(IParentService p, IStudentService s)
        {
            _parent = p;
            _student = s;
        }

        public async Task<IActionResult> Index(int page = 1, string? search = null, string? status = "Active")
        {
            var normalised = (status == "Both" || string.IsNullOrEmpty(status)) ? null : status;
            ViewBag.Search = search;
            ViewBag.Status = status ?? "Active";
            return View(await _parent.GetAllAsync(page, search, normalised).D());
        }

        public async Task<IActionResult> Details(int id)
        {
            var r = await _parent.GetByIdAsync(id);
            return r.Success ? View(r.Data) : NotFound();
        }

        [HttpGet] public IActionResult Create() => View(new CreateParentDto());
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateParentDto m)
        {
            if (!ModelState.IsValid)
                return View(m);
            var r = await _parent.CreateAsync(m, UserId);
            if (!r.Success)
            {
                ModelState.AddModelError("", r.Message);
                return View(m);
            }
            TempData["CreatedUser"] = System.Text.Json.JsonSerializer.Serialize(r.Data);
            TempData["SuccessMessage"] = "Parent created."; return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var r = await _parent.GetByIdAsync(id);
            if (!r.Success)
                return NotFound();
            ViewBag.ParentId = id;
            ViewBag.StudentMap = r.Data!.Students
                .ToDictionary(s => s.StudentCode, s => s.FullName);
            return View(new UpdateParentDto
            {
                FullName = r.Data!.FullName,
                UserName    = r.Data!.UserName,
                Email       = r.Data.Email,
                PhoneNumber = r.Data.PhoneNumber,
                IsActive = r.Data.IsActive,
                StudentCodes = r.Data.Students.Select(s => s.StudentCode).ToList()
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateParentDto m)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ParentId = id;
                return View(m);
            }
            var r = await _parent.UpdateAsync(id, m);
            if (!r.Success)
            {
                ModelState.AddModelError("", r.Message);
                ViewBag.ParentId = id;
                return View(m);
            }
            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _parent.DeleteAsync(id);
            TempData["SuccessMessage"] = "Marked inactive.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(int id)
        {
            var r = await _parent.ToggleActiveAsync(id);
            return Json(new { r.Success, r.Message });
        }

        [HttpGet]
        public async Task<IActionResult> SearchStudents(string? q)
        {
            var r = await _student.SearchAsync(q);
            return Json(r.Data);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var r = await _parent.ResetPasswordAsync(id);
            return Json(new
            {
                r.Success,
                r.Message,
                password = r.Data?.PlainPassword,
                fullName = r.Data?.FullName,
                email = r.Data?.Email
            });
        }
    }
}
