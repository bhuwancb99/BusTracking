using BusTracking.Web.Helpers;
namespace BusTracking.Web.Areas.BusCoordinator.Controllers
{
    [Area("BusCoordinator"), Authorize(Roles = "BusCoordinator")]
    public class ParentController : Controller
    {
        private readonly IParentService _parent;
        private readonly IStudentService _student;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        public ParentController(IParentService parent, IStudentService student)
        {
            _parent = parent; _student = student;
        }

        public async Task<IActionResult> Index(int page = 1, string? search = null, string? status = "Active")
        {
            if (!PermissionHelper.Can(User, "parent.view")) return Forbid();
            var normalised = (status == "Both" || string.IsNullOrEmpty(status)) ? null : status;
            ViewBag.Search = search; ViewBag.Status = status ?? "Active";
            var r = await _parent.GetAllAsync(page, 10, search, normalised);
            return View(r.Data);
        }

        public async Task<IActionResult> Details(int id)
        {
            if (!PermissionHelper.Can(User, "parent.view")) return Forbid();
            var r = await _parent.GetByIdAsync(id);
            return r.Success ? View(r.Data) : NotFound();
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!PermissionHelper.Can(User, "parent.add")) return Forbid();
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateParentDto m)
        {
            if (!PermissionHelper.Can(User, "parent.add")) return Forbid();
            if (!ModelState.IsValid) return View(m);
            var r = await _parent.CreateAsync(m, UserId);
            if (!r.Success) { ModelState.AddModelError("", r.Message); return View(m); }
            TempData["CreatedUser"] = System.Text.Json.JsonSerializer.Serialize(r.Data);
            TempData["SuccessMessage"] = "Parent created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!PermissionHelper.Can(User, "parent.edit")) return Forbid();
            var r = await _parent.GetByIdAsync(id);
            if (!r.Success) return NotFound();
            ViewBag.ParentId = id;
            ViewBag.StudentMap = r.Data!.Students.ToDictionary(s => s.StudentCode, s => s.FullName);
            return View(new UpdateParentDto
            {
                FullName     = r.Data!.FullName,
                PhoneNumber  = r.Data.PhoneNumber,
                IsActive     = r.Data.IsActive,
                StudentCodes = r.Data.Students.Select(s => s.StudentCode).ToList()
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateParentDto m)
        {
            if (!PermissionHelper.Can(User, "parent.edit")) return Forbid();
            if (!ModelState.IsValid) { ViewBag.ParentId = id; return View(m); }
            var r = await _parent.UpdateAsync(id, m);
            if (!r.Success) { ModelState.AddModelError("", r.Message); ViewBag.ParentId = id; return View(m); }
            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!PermissionHelper.Can(User, "parent.delete")) return Forbid();
            await _parent.DeleteAsync(id);
            TempData["SuccessMessage"] = "Marked inactive.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(int id)
        {
            if (!PermissionHelper.Can(User, "parent.edit"))
                return Json(new { Success = false, Message = "Permission denied." });
            var r = await _parent.ToggleActiveAsync(id);
            return Json(new { r.Success, r.Message });
        }

        [HttpGet]
        public async Task<IActionResult> SearchStudents(string? q)
        {
            var r = await _student.SearchAsync(q);
            return Json(r.Data);
        }
    }
}
