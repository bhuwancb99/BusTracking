namespace BusTracking.Web.Areas.BusCoordinator.Controllers
{
    [Area("BusCoordinator"), Authorize(Roles = "BusCoordinator")]
    public class DriverController : Controller
    {
        private readonly IDriverService _driver;
        private readonly IBusService _bus;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        public DriverController(IDriverService driver, IBusService bus)
        {
            _driver = driver;
            _bus = bus;
        }

        public async Task<IActionResult> Index(int page = 1, string? search = null, string? status = "Active")
        {
            if (!PermissionHelper.Can(User, "driver.view")) return Forbid();
            var normalised = (status == "Both" || string.IsNullOrEmpty(status)) ? null : status;
            ViewBag.Search = search;
            ViewBag.Status = status ?? "Active";
            var r = await _driver.GetAllAsync(page, search, normalised);
            return View(r.Data);
        }

        public async Task<IActionResult> Details(int id)
        {
            if (!PermissionHelper.Can(User, "driver.view")) return Forbid();
            var r = await _driver.GetByIdAsync(id);
            return r.Success ? View(r.Data) : NotFound();
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!PermissionHelper.Can(User, "driver.add")) return Forbid();
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDriverDto m)
        {
            if (!PermissionHelper.Can(User, "driver.add")) return Forbid();
            if (!ModelState.IsValid) return View(m);
            var r = await _driver.CreateAsync(m, UserId);
            if (!r.Success) { ModelState.AddModelError("", r.Message); return View(m); }
            TempData["CreatedUser"] = System.Text.Json.JsonSerializer.Serialize(r.Data);
            TempData["SuccessMessage"] = "Driver created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!PermissionHelper.Can(User, "driver.edit")) return Forbid();
            var r = await _driver.GetByIdAsync(id);
            if (!r.Success) return NotFound();
            ViewBag.DriverId = id;
            if (r.Data!.BusId.HasValue && r.Data.BusName != null)
                ViewBag.BusDisplay = $"{r.Data.BusName} ({r.Data.BusNumber})";
            return View(new UpdateDriverDto
            {
                FullName      = r.Data!.FullName,
                UserName    = r.Data!.UserName,
                Email       = r.Data.Email,
                PhoneNumber   = r.Data.PhoneNumber,
                LicenseNumber = r.Data.LicenseNumber,
                LicenseExpiry = r.Data.LicenseExpiry,
                BusId         = r.Data.BusId,
                IsActive      = r.Data.IsActive
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateDriverDto m)
        {
            if (!PermissionHelper.Can(User, "driver.edit")) return Forbid();
            if (!ModelState.IsValid) { ViewBag.DriverId = id; return View(m); }
            var r = await _driver.UpdateAsync(id, m);
            if (!r.Success) { ModelState.AddModelError("", r.Message); ViewBag.DriverId = id; return View(m); }
            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!PermissionHelper.Can(User, "driver.delete")) return Forbid();
            await _driver.DeleteAsync(id);
            TempData["SuccessMessage"] = "Marked inactive.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(int id)
        {
            if (!PermissionHelper.Can(User, "driver.edit"))
                return Json(new { Success = false, Message = "Permission denied." });
            var r = await _driver.ToggleActiveAsync(id);
            return Json(new { r.Success, r.Message });
        }

        [HttpGet]
        public async Task<IActionResult> SearchBuses(string? q)
        {
            var r = await _bus.GetDropdownAsync(q);
            return Json(r.Data);
        }

        [HttpPost]
        public async Task<IActionResult> AssignBus([FromBody] AssignBusToDriverDto dto)
        {
            if (!PermissionHelper.Can(User, "driver.edit"))
                return Json(new { Success = false, Message = "Permission denied." });
            var r = await _driver.AssignBusAsync(dto);
            return Json(new { r.Success, r.Message });
        }
    }
}
