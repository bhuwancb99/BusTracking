namespace BusTracking.Web.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin"), Authorize(Roles = "SuperAdmin")]
    public class SubAdminController : Controller
    {
        private readonly ISubAdminService _sa;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        public SubAdminController(ISubAdminService sa) => _sa = sa;

        public async Task<IActionResult> Index(int page = 1, string? search = null, string? status = "Active")
        {
            ViewBag.Search = search;
            ViewBag.Status = status;
            return View(await _sa.GetAllAsync(page, 10, search, status).D());
        }

        public async Task<IActionResult> Details(int id)
        {
            var r = await _sa.GetByIdAsync(id);
            return r.Success ? View(r.Data) : NotFound();
        }

        [HttpGet] public IActionResult Create() => View(new CreateSubAdminDto());
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateSubAdminDto m)
        {
            if (!ModelState.IsValid)
                return View(m);
            var r = await _sa.CreateAsync(m, UserId);
            if (!r.Success)
            {
                ModelState.AddModelError("", r.Message);
                return View(m);
            }
            TempData["CreatedUser"] = System.Text.Json.JsonSerializer.Serialize(r.Data);
            TempData["SuccessMessage"] = "Coordinator created."; return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var r = await _sa.GetByIdAsync(id);
            if (!r.Success)
                return NotFound();
            ViewBag.SubAdminId = id;
            return View(new UpdateSubAdminDto
            {
                FullName = r.Data!.FullName,
                PhoneNumber = r.Data.PhoneNumber,
                IsActive = r.Data.IsActive,
                PermissionIds = []
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateSubAdminDto m)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.SubAdminId = id;
                return View(m);
            }
            var r = await _sa.UpdateAsync(id, m);
            if (!r.Success)
            {
                ModelState.AddModelError("", r.Message);
                ViewBag.SubAdminId = id;
                return View(m);
            }
            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _sa.DeleteAsync(id);
            TempData["SuccessMessage"] = "Marked inactive.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(int id)
        {
            var r = await _sa.ToggleActiveAsync(id);
            return Json(new { r.Success, r.Message });
        }
    }
}
