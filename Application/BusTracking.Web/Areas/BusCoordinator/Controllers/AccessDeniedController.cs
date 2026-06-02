namespace BusTracking.Web.Areas.BusCoordinator.Controllers
{
    [Area("BusCoordinator"), Authorize(Roles = "BusCoordinator")]
    public class AccessDeniedController : Controller
    {
        // GET /BusCoordinator/AccessDenied
        // Called directly by Forbid() via the AccessDeniedPath cookie option,
        // or redirected to from any controller action that detects missing permission.
        public IActionResult Index(string? returnUrl = null)
        {
            // Derive a human-readable page name from the returnUrl if available
            string page = "this page";
            if (!string.IsNullOrEmpty(returnUrl))
            {
                // e.g. /BusCoordinator/Bus/Index → "Bus"
                var segments = returnUrl.Trim('/').Split('/');
                // Area/Controller/Action — pick the controller segment (index 1)
                if (segments.Length >= 2 && !string.IsNullOrWhiteSpace(segments[1]))
                    page = segments[1];
            }

            ViewBag.RequestedPage = page;
            return View();
        }
    }
}
