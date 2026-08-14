namespace BusTracking.Web.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin"), Authorize(Roles = "SuperAdmin")]
    public class StudentController : Controller
    {
        private readonly IStudentService _student;
        private readonly ISectionService _section;
        private readonly IBusService _bus;
        private readonly IRouteService _route;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        public StudentController(IStudentService s, ISectionService sec, IBusService b, IRouteService r)
        {
            _student = s;
            _section = sec;
            _bus = b;
            _route = r;
        }

        public async Task<IActionResult> Index(int page = 1, string? search = null, string? status = "Active")
        {
            var normalised = (status == "Both" || string.IsNullOrEmpty(status)) ? null : status;
            ViewBag.Status = status;
            return View(await _student.GetAllAsync(page, search, normalised).D());
        }

        public async Task<IActionResult> Details(int id)
        {
            var r = await _student.GetByIdAsync(id);
            return r.Success ? View(r.Data) : NotFound();
        }

        private async Task PopulateStandardsAndSectionsAsync(int? standardId = null, int? sectionId = null)
        {
            var standardsRes = await _student.GetStandardsAsync();
            var standards = standardsRes.Success ? standardsRes.Data : new List<StandardMaster>();
            ViewBag.Standards = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(standards, "StandardId", "StandardName", standardId);

            var sections = new List<SectionDto>();
            if (standardId.HasValue && standardId.Value > 0)
            {
                var secRes = await _section.GetSectionsByStandardAsync(standardId.Value);
                sections = secRes.Data ?? new List<SectionDto>();
            }
            var formattedSections = sections.Select(s => new { s.SectionId, SectionName = "Section " + s.SectionName }).ToList();
            ViewBag.Sections = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(formattedSections, "SectionId", "SectionName", sectionId);
        }


        [HttpGet]
        public async Task<IActionResult> GetSectionsByStandard(int standardId)
        {
            var res = await _section.GetSectionsByStandardAsync(standardId);
            return Json(res.Data ?? new());
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateStandardsAndSectionsAsync();
            return View(new CreateStudentDto());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateStudentDto m)
        {
            if (!ModelState.IsValid)
            {
                await PopulateStandardsAndSectionsAsync(m.StandardId, m.SectionId);
                return View(m);
            }
            var r = await _student.CreateAsync(m, UserId);
            if (!r.Success)
            {
                ModelState.AddModelError("", r.Message);
                await PopulateStandardsAndSectionsAsync(m.StandardId, m.SectionId);
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
            if (r.Data!.BusId.HasValue && r.Data.BusName != null)
                ViewBag.BusDisplay = $"{r.Data.BusName} ({r.Data.BusNumber})";
            await PopulateStandardsAndSectionsAsync(r.Data.StandardId, r.Data.SectionId);
            return View(new UpdateStudentDto
            {
                FullName = r.Data!.FullName,
                UserName = r.Data!.UserName,
                Email = r.Data.Email,
                PhoneNumber = r.Data.PhoneNumber,
                StudentCode = r.Data.StudentCode,
                StandardId = r.Data.StandardId,
                SectionId = r.Data.SectionId,
                BusId = r.Data.BusId,
                StopId = r.Data.StopId,
                TransportFeeStatus = r.Data.TransportFeeStatus,
                FeeExpiryDate = r.Data.FeeExpiryDate,
                IsActive = r.Data.IsActive
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateStudentDto m)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.StudentId = id;
                await PopulateStandardsAndSectionsAsync(m.StandardId, m.SectionId);
                return View(m);
            }
            var r = await _student.UpdateAsync(id, m);
            if (!r.Success)
            {
                ModelState.AddModelError("", r.Message);
                ViewBag.StudentId = id;
                await PopulateStandardsAndSectionsAsync(m.StandardId, m.SectionId);
                return View(m);
            }
            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int id)
        {
            var r = await _student.ToggleActiveAsync(id);
            return Json(new { success = r.Success, message = r.Message });
        }

        [HttpGet]
        public async Task<IActionResult> SearchBuses(string q)
        {
            var r = await _bus.GetDropdownAsync(q);
            return Json(r.Data ?? new());
        }


        [HttpGet]
        public async Task<IActionResult> SearchStops(int busId)
        {
            var r = await _route.GetStopsByBusAsync(busId);
            return Json(r.Data ?? new());
        }
    }
}
