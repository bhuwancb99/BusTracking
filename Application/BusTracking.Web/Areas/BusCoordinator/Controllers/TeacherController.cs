namespace BusTracking.Web.Areas.BusCoordinator.Controllers
{
    [Area("BusCoordinator")]
    [Authorize(Roles = "BusCoordinator")]
    public class TeacherController : Controller
    {
        private readonly ITeacherService _teacherService;
        private readonly IImageService _imageService;

        private int CurrentSchoolId => int.TryParse(User.FindFirst("SchoolId")?.Value, out var sid) ? sid : 1;

        public TeacherController(ITeacherService teacherService, IImageService imageService)
        {
            _teacherService = teacherService;
            _imageService = imageService;
        }

        public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 10)
        {
            if (!PermissionHelper.Can(User, "teachers.view", HttpContext))
                return Forbid();

            var result = await _teacherService.GetTeachersAsync(CurrentSchoolId, search, page, pageSize);

            ViewBag.Search = search;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.TotalItems = result.TotalCount;

            return View(result);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!PermissionHelper.Can(User, "teachers.add", HttpContext))
                return Forbid();

            return View(new CreateTeacherDto { SchoolId = CurrentSchoolId, IsActive = true });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTeacherDto model, IFormFile? profileImage)
        {
            if (!PermissionHelper.Can(User, "teachers.add", HttpContext))
                return Forbid();

            model.SchoolId = CurrentSchoolId;

            if (!ModelState.IsValid)
                return View(model);

            string? avatarUrl = null;
            if (profileImage != null && profileImage.Length > 0)
            {
                avatarUrl = await _imageService.SaveProfileImageAsync(profileImage, 0, "Teacher", null);
            }

            var result = await _teacherService.CreateTeacherAsync(model, avatarUrl);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View(model);
            }

            TempData["SuccessMessage"] = "Teacher registered successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!PermissionHelper.Can(User, "teachers.edit", HttpContext))
                return Forbid();

            var result = await _teacherService.GetTeacherByIdAsync(id);
            if (!result.Success || result.Data == null) return NotFound();

            var dto = new UpdateTeacherDto
            {
                TeacherId = result.Data.TeacherId,
                FullName = result.Data.FullName,
                UserName = result.Data.UserName,
                Email = result.Data.Email,
                PhoneNumber = result.Data.PhoneNumber,
                EmployeeCode = result.Data.EmployeeCode,
                Qualification = result.Data.Qualification,
                Designation = result.Data.Designation,
                Department = result.Data.Department,
                JoiningDate = result.Data.JoiningDate,
                Gender = result.Data.Gender,
                EmergencyContact = result.Data.EmergencyContact,
                IsActive = result.Data.IsActive
            };

            ViewBag.ProfileImageUrl = result.Data.ProfileImageUrl;
            ViewBag.UserId = result.Data.UserId;

            return View(dto);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateTeacherDto model, IFormFile? profileImage)
        {
            if (!PermissionHelper.Can(User, "teachers.edit", HttpContext))
                return Forbid();

            if (!ModelState.IsValid)
                return View(model);

            string? avatarUrl = null;
            if (profileImage != null && profileImage.Length > 0)
            {
                avatarUrl = await _imageService.SaveProfileImageAsync(profileImage, 0, "Teacher", null);
            }

            var result = await _teacherService.UpdateTeacherAsync(model, avatarUrl);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View(model);
            }

            TempData["SuccessMessage"] = "Teacher updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            if (!PermissionHelper.Can(User, "teachers.edit", HttpContext))
                return Forbid();

            var result = await _teacherService.ToggleTeacherStatusAsync(id);
            return Json(new { success = result.Success, isActive = result.Data, message = result.Message });
        }

        [HttpGet]
        public async Task<IActionResult> CheckUsername(string userName, int? excludeUserId)
        {
            var result = await _teacherService.CheckUsernameAvailabilityAsync(userName, excludeUserId);
            return Json(new { success = result.Success, message = result.Message });
        }
    }
}
