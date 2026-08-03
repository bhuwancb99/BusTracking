namespace BusTracking.Web.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
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
        public async Task<IActionResult> Details(int id)
        {
            if (!PermissionHelper.Can(User, "teachers.view", HttpContext))
                return Forbid();

            var result = await _teacherService.GetTeacherByIdAsync(id);
            if (!result.Success || result.Data == null) return NotFound();

            return View(result.Data);
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

            if (string.IsNullOrWhiteSpace(model.Password))
            {
                model.Password = "Teacher123!";
            }

            if (!ModelState.IsValid)
                return View(model);

            var result = await _teacherService.CreateTeacherAsync(model, null);
            if (!result.Success || result.Data == null)
            {
                ModelState.AddModelError("", result.Message);
                return View(model);
            }

            if (profileImage != null && profileImage.Length > 0)
            {
                try
                {
                    var avatarUrl = await _imageService.SaveProfileImageAsync(profileImage, result.Data.UserId, "Teacher", null);
                    var updateDto = new UpdateTeacherDto
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
                    await _teacherService.UpdateTeacherAsync(updateDto, avatarUrl);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Teacher created, but image upload failed: " + ex.Message);
                }
            }

            TempData["CreatedUser"] = System.Text.Json.JsonSerializer.Serialize(new CreatedUserResultDto
            {
                FullName = result.Data.FullName,
                Role = "Teacher",
                Email = string.IsNullOrEmpty(result.Data.Email) ? result.Data.UserName : result.Data.Email,
                PlainPassword = model.Password
            });

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
                try
                {
                    var teacherRecord = await _teacherService.GetTeacherByIdAsync(model.TeacherId);
                    if (teacherRecord.Success && teacherRecord.Data != null)
                    {
                        avatarUrl = await _imageService.SaveProfileImageAsync(profileImage, teacherRecord.Data.UserId, "Teacher", teacherRecord.Data.ProfileImageUrl);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Image upload failed: " + ex.Message);
                    return View(model);
                }
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

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int id)
        {
            if (!PermissionHelper.Can(User, "teachers.edit", HttpContext))
                return Json(new { success = false, message = "Access denied." });

            var teacher = await _teacherService.GetTeacherByIdAsync(id);
            if (!teacher.Success || teacher.Data == null)
                return Json(new { success = false, message = "Teacher not found." });

            string newPassword = "Pass" + Random.Shared.Next(100000, 999999) + "!";
            var updateDto = new UpdateTeacherDto
            {
                TeacherId = teacher.Data.TeacherId,
                FullName = teacher.Data.FullName,
                UserName = teacher.Data.UserName,
                Email = teacher.Data.Email,
                PhoneNumber = teacher.Data.PhoneNumber,
                EmployeeCode = teacher.Data.EmployeeCode,
                Qualification = teacher.Data.Qualification,
                Designation = teacher.Data.Designation,
                Department = teacher.Data.Department,
                JoiningDate = teacher.Data.JoiningDate,
                Gender = teacher.Data.Gender,
                EmergencyContact = teacher.Data.EmergencyContact,
                Password = newPassword,
                IsActive = teacher.Data.IsActive
            };

            var r = await _teacherService.UpdateTeacherAsync(updateDto);
            return Json(new
            {
                success = r.Success,
                message = r.Message,
                password = newPassword,
                fullName = teacher.Data.FullName,
                email = string.IsNullOrEmpty(teacher.Data.Email) ? teacher.Data.UserName : teacher.Data.Email
            });
        }

        [HttpGet]
        public async Task<IActionResult> CheckUsername(string userName, int? excludeUserId)
        {
            var result = await _teacherService.CheckUsernameAvailabilityAsync(userName, excludeUserId);
            return Json(new { success = result.Success, message = result.Message });
        }
    }
}
