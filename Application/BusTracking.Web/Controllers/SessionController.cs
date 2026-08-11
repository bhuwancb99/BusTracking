namespace BusTracking.Web.Controllers
{
    [Authorize]
    public class SessionController : Controller
    {
        private readonly IAcademicYearService _academicYearService;
        private readonly ICurrentUserService _currentUser;

        public SessionController(IAcademicYearService academicYearService, ICurrentUserService currentUser)
        {
            _academicYearService = academicYearService;
            _currentUser = currentUser;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Switch(int id, string? returnUrl)
        {
            int schoolId = _currentUser.SchoolId ?? 1;
            var userName = User.Identity?.Name ?? User.GetFullName() ?? "User";
            var result = await _academicYearService.SetActiveAcademicYearAsync(schoolId, id, userName);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Active Academic Session switched successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Dashboard");
        }
    }
}
