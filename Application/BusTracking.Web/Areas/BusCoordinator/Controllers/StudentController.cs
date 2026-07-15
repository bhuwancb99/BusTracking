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
            _s = s; _bus = bus; _route = route;
        }

        public async Task<IActionResult> Index(int page = 1, string? search = null, string? status = "Active")
        {
            if (!PermissionHelper.Can(User, "student.view")) return Forbid();
            var normalised = (status == "Both" || string.IsNullOrEmpty(status)) ? null : status;
            ViewBag.Search = search; ViewBag.Status = status ?? "Active";
            var r = await _s.GetAllAsync(page, search, normalised);
            return View(r.Data);
        }

        public async Task<IActionResult> Details(int id)
        {
            if (!PermissionHelper.Can(User, "student.view")) return Forbid();
            var r = await _s.GetByIdAsync(id);
            return r.Success ? View(r.Data) : NotFound();
        }

        private async Task PopulateStandardsAsync()
        {
            var standardsRes = await _s.GetStandardsAsync();
            var standards = standardsRes.Success ? standardsRes.Data : new List<StandardMaster>();
            ViewBag.Standards = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(standards, "StandardId", "StandardName");
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!PermissionHelper.Can(User, "student.add")) return Forbid();
            await PopulateStandardsAsync();
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateStudentDto m)
        {
            if (!PermissionHelper.Can(User, "student.add")) return Forbid();
            if (!ModelState.IsValid)
            {
                await PopulateStandardsAsync();
                return View(m);
            }
            var r = await _s.CreateAsync(m, UserId);
            if (!r.Success)
            {
                ModelState.AddModelError("", r.Message);
                await PopulateStandardsAsync();
                return View(m);
            }
            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!PermissionHelper.Can(User, "student.edit")) return Forbid();
            var r = await _s.GetByIdAsync(id);
            if (!r.Success) return NotFound();
            ViewBag.StudentId = id;
            if (r.Data!.BusId.HasValue && r.Data.BusName != null)
                ViewBag.BusDisplay = $"{r.Data.BusName} ({r.Data.BusNumber})";
            await PopulateStandardsAsync();
            return View(new UpdateStudentDto
            {
                FullName = r.Data!.FullName,
                UserName = r.Data!.UserName,
                Email = r.Data.Email,
                PhoneNumber = r.Data.PhoneNumber,
                StudentCode = r.Data.StudentCode,
                StandardId = r.Data.StandardId,
                BusId = r.Data.BusId,
                StopId = r.Data.StopId,
                IsActive = r.Data.IsActive
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateStudentDto m)
        {
            if (!PermissionHelper.Can(User, "student.edit")) return Forbid();
            if (!ModelState.IsValid)
            {
                ViewBag.StudentId = id;
                await PopulateStandardsAsync();
                return View(m);
            }
            var r = await _s.UpdateAsync(id, m);
            if (!r.Success)
            {
                ModelState.AddModelError("", r.Message);
                ViewBag.StudentId = id;
                await PopulateStandardsAsync();
                return View(m);
            }
            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!PermissionHelper.Can(User, "student.delete")) return Forbid();
            await _s.DeleteAsync(id);
            TempData["SuccessMessage"] = "Marked inactive.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(int id)
        {
            if (!PermissionHelper.Can(User, "student.edit"))
                return Json(new { Success = false, Message = "Permission denied." });
            var r = await _s.ToggleActiveAsync(id);
            return Json(new { r.Success, r.Message });
        }

        [HttpPost]
        public async Task<IActionResult> AssignBus([FromBody] BusTracking.Common.DTOs.Assign.AssignBusToStudentDto dto)
        {
            if (!PermissionHelper.Can(User, "student.assignbus"))
                return Json(new { Success = false, Message = "Permission denied." });
            var r = await _s.AssignBusAsync(dto);
            return Json(new { r.Success, r.Message });
        }

        [HttpGet] public async Task<IActionResult> SearchBuses(string? q) { var r = await _bus.GetDropdownAsync(q); return Json(r.Data); }
        [HttpGet] public async Task<IActionResult> Search(string? q) { var r = await _s.SearchAsync(q); return Json(r.Data); }

        [HttpGet]
        public async Task<IActionResult> SearchStops(int busId)
        {
            var r = await _route.GetStopsByBusAsync(busId);
            if (!r.Success) return Json(Array.Empty<object>());
            return Json((r.Data ?? []).OrderBy(s => s.StopOrder).Select(s => new
            {
                stopId = s.StopId,
                stopName = s.StopName,
                stopOrder = s.StopOrder,
                morningTime = s.MorningTime,
                eveningTime = s.EveningTime
            }));
        }

        public async Task<IActionResult> Availability(int studentId)
        {
            if (!PermissionHelper.Can(User, "student.view")) return Forbid();
            var r = await _s.GetAvailabilitiesAsync(studentId);
            ViewBag.StudentId = studentId;
            return View(r.Data);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SetAvailability(CreateAvailabilityDto m)
        {
            if (!PermissionHelper.Can(User, "student.edit")) return Forbid();
            var r = await _s.SetAvailabilityAsync(m, UserId);
            TempData[r.Success ? "SuccessMessage" : "ErrorMessage"] = r.Message;
            return RedirectToAction(nameof(Availability), new { studentId = m.StudentId });
        }
    }
}
