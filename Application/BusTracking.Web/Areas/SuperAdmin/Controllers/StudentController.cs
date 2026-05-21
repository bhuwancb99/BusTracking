using Microsoft.AspNetCore.Mvc;

namespace BusTracking.Web.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin"), Authorize(Roles = "SuperAdmin")]
    public class StudentController : Controller
    {
        private readonly IStudentService _student;
        private readonly IBusService _bus;
        private readonly IRouteService _route;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        public StudentController(IStudentService s, IBusService b, IRouteService r)
        {
            _student = s;
            _bus = b;
            _route = r;
        }

        public async Task<IActionResult> Index(int page = 1, string? search = null, string? status = "Active")
        {
            ViewBag.Search = search;
            ViewBag.Status = status;
            return View(await _student.GetAllAsync(page, 10, search, status));
        }

        public async Task<IActionResult> Details(int id)
        {
            var r = await _student.GetByIdAsync(id);
            return r.Success ? View(r.Data) : NotFound();
        }

        [HttpGet] public IActionResult Create() => View(new CreateStudentDto());
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateStudentDto m)
        {
            if (!ModelState.IsValid)
                return View(m);
            var r = await _student.CreateAsync(m, UserId);
            if (!r.Success)
            {
                ModelState.AddModelError("", r.Message);
                return View(m);
            }
            TempData["CreatedUser"] = System.Text.Json.JsonSerializer.Serialize(r.Data);
            TempData["SuccessMessage"] = "Student created."; return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var r = await _student.GetByIdAsync(id);
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
            var r = await _student.UpdateAsync(id, m);
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
            await _student.DeleteAsync(id);
            TempData["SuccessMessage"] = "Marked inactive.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(int id)
        {
            var r = await _student.ToggleActiveAsync(id);
            return Json(new { r.Success, r.Message });
        }

        [HttpPost]
        public async Task<IActionResult> AssignBus([FromBody] AssignBusToStudentDto dto)
        {
            var r = await _student.AssignBusAsync(dto);
            return Json(new { r.Success, r.Message });
        }

        [HttpGet]
        public async Task<IActionResult> SearchBuses(string? q)
        {
            var r = await _bus.GetDropdownAsync(q);
            return Json(r.Data);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string? q)
        {
            var r = await _student.SearchAsync(q);
            return Json(r.Data);
        }

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
            var r = await _student.GetAvailabilitiesAsync(studentId);
            ViewBag.StudentId = studentId;
            return View(r.Data);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SetAvailability(CreateAvailabilityDto m)
        {
            var r = await _student.SetAvailabilityAsync(m, UserId);
            TempData[r.Success ? "SuccessMessage" : "ErrorMessage"] = r.Message;
            return RedirectToAction(nameof(Availability), new { studentId = m.StudentId });
        }
    }
}
