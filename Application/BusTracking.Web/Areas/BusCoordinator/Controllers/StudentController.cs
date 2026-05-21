namespace BusTracking.Web.Areas.BusCoordinator.Controllers
{
    [Area("BusCoordinator"), Authorize(Roles = "BusCoordinator")]
    public class StudentController : Controller
    {
        private readonly IStudentService _s;
        private readonly IBusService _bus;
        private readonly IRouteService _route;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        public StudentController(IStudentService s, IBusService bus, IRouteService route)
        {
            _s = s;
            _bus = bus;
            _route = route;
        }

        public async Task<IActionResult> Index(int page = 1, string? search = null, string? status = "Active")
        {
            ViewBag.Search = search; ViewBag.Status = status;
            var r = await _s.GetAllAsync(page, 10, search, status);
            return View(r.Data);
        }
        [HttpGet] public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateStudentDto m)
        {
            if (!ModelState.IsValid)
                return View(m);
            var r = await _s.CreateAsync(m, UserId);
            if (!r.Success)
            {
                ModelState.AddModelError("", r.Message);
                return View(m);
            }
            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var r = await _s.GetByIdAsync(id);
            if (!r.Success)
                return NotFound();
            ViewBag.StudentId = id;
            return View(new UpdateStudentDto
            {
                FullName = r.Data!.FullName,
                PhoneNumber = r.Data.PhoneNumber,
                StudentCode = r.Data.StudentCode,
                Standard = r.Data.Standard,
                BusId = r.Data.BusId,
                StopId = r.Data.StopId,
                IsActive = r.Data.IsActive
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateStudentDto m)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.StudentId = id;
                return View(m);
            }
            var r = await _s.UpdateAsync(id, m);
            if (!r.Success)
            {
                ModelState.AddModelError("", r.Message);
                ViewBag.StudentId = id;
                return View(m);
            }
            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _s.DeleteAsync(id);
            TempData["SuccessMessage"] = "Marked inactive.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var r = await _s.GetByIdAsync(id);
            return r.Success ? View(r.Data) : NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(int id)
        {
            var r = await _s.ToggleActiveAsync(id);
            return Json(new { r.Success, r.Message });
        }

        [HttpPost]
        public async Task<IActionResult> AssignBus([FromBody] BusTracking.Common.DTOs.Assign.AssignBusToStudentDto dto)
        {
            var r = await _s.AssignBusAsync(dto);
            return Json(new { r.Success, r.Message });
        }

        // Autocomplete endpoints
        [HttpGet]
        public async Task<IActionResult> SearchBuses(string? q)
        {
            var r = await _bus.GetDropdownAsync(q);
            return Json(r.Data);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string? q)
        {
            var r = await _s.SearchAsync(q);
            return Json(r.Data);
        }

        // SearchStops — loads stops for a bus's assigned route
        [HttpGet]
        public async Task<IActionResult> SearchStops(int busId)
        {
            var r = await _route.GetStopsByBusAsync(busId);
            if (!r.Success) return Json(Array.Empty<object>());
            var list = (r.Data ?? [])
                .OrderBy(s => s.StopOrder)
                .Select(s => new
                {
                    stopId = s.StopId,
                    stopName = s.StopName,
                    stopOrder = s.StopOrder,
                    morningTime = s.MorningTime,
                    eveningTime = s.EveningTime
                });
            return Json(list);
        }

        public async Task<IActionResult> Availability(int studentId)
        {
            var r = await _s.GetAvailabilitiesAsync(studentId);
            ViewBag.StudentId = studentId; return View(r.Data);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SetAvailability(CreateAvailabilityDto m)
        {
            var r = await _s.SetAvailabilityAsync(m, UserId);
            TempData[r.Success ? "SuccessMessage" : "ErrorMessage"] = r.Message;
            return RedirectToAction(nameof(Availability), new { studentId = m.StudentId });
        }
    }
}
