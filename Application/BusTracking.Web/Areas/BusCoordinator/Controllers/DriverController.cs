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
            // Normalise: "Both" and null both mean no status filter for the service
            var normalised = (status == "Both" || string.IsNullOrEmpty(status)) ? null : status;
            ViewBag.Search = search;
            ViewBag.Status = status ?? "Active"; // keep selection stable; default to Active on first load
            var r = await _driver.GetAllAsync(page, 10, search, normalised);
            return View(r.Data);
        }
        public async Task<IActionResult> Details(int id)
        {
            var r = await _driver.GetByIdAsync(id);
            return r.Success ? View(r.Data) : NotFound();
        }

        [HttpGet] public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDriverDto m)
        {
            if (!ModelState.IsValid)
                return View(m);
            var r = await _driver.CreateAsync(m, UserId);
            if (!r.Success)
            {
                ModelState.AddModelError("", r.Message);
                return View(m);
            }
            TempData["CreatedUser"] = System.Text.Json.JsonSerializer.Serialize(r.Data);
            TempData["SuccessMessage"] = "Driver created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var r = await _driver.GetByIdAsync(id);
            if (!r.Success)
                return NotFound();
            ViewBag.DriverId = id;
            // Pass the human-readable bus display so the textbox shows name not ID
            if (r.Data!.BusId.HasValue && r.Data.BusName != null)
                ViewBag.BusDisplay = $"{r.Data.BusName} ({r.Data.BusNumber})";
            return View(new UpdateDriverDto
            {
                FullName = r.Data!.FullName,
                PhoneNumber = r.Data.PhoneNumber,
                LicenseNumber = r.Data.LicenseNumber,
                LicenseExpiry = r.Data.LicenseExpiry,
                BusId = r.Data.BusId,
                IsActive = r.Data.IsActive
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateDriverDto m)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.DriverId = id;
                return View(m);
            }
            var r = await _driver.UpdateAsync(id, m);
            if (!r.Success)
            {
                ModelState.AddModelError("", r.Message);
                ViewBag.DriverId = id; return View(m);
            }
            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _driver.DeleteAsync(id);
            TempData["SuccessMessage"] = "Marked inactive.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(int id)
        {
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
            var r = await _driver.AssignBusAsync(dto);
            return Json(new { r.Success, r.Message });
        }
    }
}