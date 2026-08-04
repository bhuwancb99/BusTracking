namespace BusTracking.Web.Areas.BusCoordinator.Controllers;

[Area("BusCoordinator")]
[Authorize]
public class AcademicYearController : Controller
{
    private readonly IAcademicYearService _academicYearService;
    private readonly ICurrentUserService _currentUser;

    public AcademicYearController(IAcademicYearService academicYearService, ICurrentUserService currentUser)
    {
        _academicYearService = academicYearService;
        _currentUser = currentUser;
    }

    private int GetSchoolId() => _currentUser.SchoolId ?? 1;
    private string GetUserName() => User.Identity?.Name ?? User.GetFullName() ?? "Coordinator";

    private bool CheckPermission(string permissionKey)
    {
        return PermissionHelper.Can(User, permissionKey, HttpContext);
    }

    public async Task<IActionResult> Index()
    {
        if (!CheckPermission("academicyear.view"))
            return RedirectToAction("AccessDenied", "Auth", new { area = "" });

        int schoolId = GetSchoolId();
        var years = await _academicYearService.GetAcademicYearsAsync(schoolId, activeOnly: false);
        return View(years);
    }

    public IActionResult Create()
    {
        if (!CheckPermission("academicyear.add"))
            return RedirectToAction("AccessDenied", "Auth", new { area = "" });

        var model = new CreateAcademicYearRequest
        {
            SchoolId = GetSchoolId(),
            StartDate = new DateTime(DateTime.Today.Year, 4, 1),
            EndDate = new DateTime(DateTime.Today.Year + 1, 3, 31),
            IsActive = true,
            SetAsCurrent = true
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAcademicYearRequest request)
    {
        if (!CheckPermission("academicyear.add"))
            return RedirectToAction("AccessDenied", "Auth", new { area = "" });

        request.SchoolId = GetSchoolId();
        if (!ModelState.IsValid) return View(request);

        var result = await _academicYearService.CreateAcademicYearAsync(request, GetUserName());
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Failed to create academic year.");
            return View(request);
        }

        TempData["SuccessMessage"] = $"Academic Year '{request.YearName}' created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        if (!CheckPermission("academicyear.edit"))
            return RedirectToAction("AccessDenied", "Auth", new { area = "" });

        var year = await _academicYearService.GetByIdAsync(id);
        if (year is null) return NotFound();

        var model = new UpdateAcademicYearRequest
        {
            AcademicYearId = year.AcademicYearId,
            YearName = year.YearName,
            StartDate = year.StartDate,
            EndDate = year.EndDate,
            IsActive = year.IsActive,
            SetAsCurrent = year.IsCurrent
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateAcademicYearRequest request)
    {
        if (!CheckPermission("academicyear.edit"))
            return RedirectToAction("AccessDenied", "Auth", new { area = "" });

        if (!ModelState.IsValid) return View(request);

        var result = await _academicYearService.UpdateAcademicYearAsync(request, GetUserName());
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Failed to update academic year.");
            return View(request);
        }

        TempData["SuccessMessage"] = "Academic Year updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetActive(int id)
    {
        if (!CheckPermission("academicyear.edit"))
            return RedirectToAction("AccessDenied", "Auth", new { area = "" });

        int schoolId = GetSchoolId();
        var result = await _academicYearService.SetActiveAcademicYearAsync(schoolId, id, GetUserName());
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Active Academic Session updated successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        if (!CheckPermission("academicyear.edit"))
            return RedirectToAction("AccessDenied", "Auth", new { area = "" });

        var result = await _academicYearService.ToggleAcademicYearStatusAsync(id, GetUserName());
        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SwitchSession(int id, string? returnUrl)
    {
        int schoolId = GetSchoolId();
        var result = await _academicYearService.SetActiveAcademicYearAsync(schoolId, id, GetUserName());
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Switched active academic session successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction(nameof(Index));
    }
}
