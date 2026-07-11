using BusTracking.Web.Helpers;
namespace BusTracking.Web.Areas.BusCoordinator.Controllers
{
    [Area("BusCoordinator"), Authorize(Roles = "BusCoordinator")]
    public class RouteController : Controller
    {
        private readonly IRouteService _route;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        public RouteController(IRouteService r) => _route = r;

        public async Task<IActionResult> Index(int page = 1, string? search = null, string? status = "Active")
        {
            if (!PermissionHelper.Can(User, "route.view")) return Forbid();
            var normalised = (status == "Both" || string.IsNullOrEmpty(status)) ? null : status;
            ViewBag.Search = search;
            ViewBag.Status = status ?? "Active";
            var r = await _route.GetAllAsync(page, search, normalised);
            return View(r.Data);
        }

        public async Task<IActionResult> Details(int id)
        {
            if (!PermissionHelper.Can(User, "route.view")) return Forbid();
            var r = await _route.GetByIdAsync(id);
            return r.Success ? View(r.Data) : NotFound();
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!PermissionHelper.Can(User, "route.add")) return Forbid();
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRouteDto m)
        {
            if (!PermissionHelper.Can(User, "route.add")) return Forbid();
            if (!ModelState.IsValid) return View(m);
            var r = await _route.CreateAsync(m, UserId);
            if (!r.Success) { ModelState.AddModelError("", r.Message); return View(m); }
            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!PermissionHelper.Can(User, "route.edit")) return Forbid();
            var r = await _route.GetByIdAsync(id);
            if (!r.Success) return NotFound();
            ViewBag.RouteId = id;
            return View(new UpdateRouteDto
            {
                RouteName = r.Data!.RouteName,
                RouteCode = r.Data.RouteCode,
                MorningTime = r.Data.MorningTime,
                EveningTime = r.Data.EveningTime,
                Description = r.Data.Description
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateRouteDto m)
        {
            if (!PermissionHelper.Can(User, "route.edit")) return Forbid();
            if (!ModelState.IsValid) { ViewBag.RouteId = id; return View(m); }
            var r = await _route.UpdateAsync(id, m);
            if (!r.Success) { ModelState.AddModelError("", r.Message); ViewBag.RouteId = id; return View(m); }
            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!PermissionHelper.Can(User, "route.delete")) return Forbid();
            await _route.DeleteAsync(id);
            TempData["SuccessMessage"] = "Deleted.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(int id)
        {
            if (!PermissionHelper.Can(User, "route.edit"))
                return Json(new { Success = false, Message = "Permission denied." });
            var r = await _route.ToggleActiveAsync(id);
            return Json(new { r.Success, r.Message });
        }

        [HttpPost]
        public async Task<IActionResult> AddStop([FromBody] CreateStopDto dto)
        {
            if (!PermissionHelper.Can(User, "route.edit"))
                return Json(new { Success = false, Message = "Permission denied." });
            var r = await _route.AddStopAsync(dto);
            return Json(new { r.Success, r.Message });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStop(int stopId)
        {
            if (!PermissionHelper.Can(User, "route.edit"))
                return Json(new { Success = false, Message = "Permission denied." });
            var r = await _route.DeleteStopAsync(stopId);
            return Json(new { r.Success, r.Message });
        }

        [HttpPost]
        public async Task<IActionResult> ReorderStops([FromBody] ReorderStopsDto dto)
        {
            if (!PermissionHelper.Can(User, "route.edit"))
                return Json(new { Success = false, Message = "Permission denied." });
            var r = await _route.ReorderStopsAsync(dto);
            return Json(new { r.Success, r.Message });
        }
    }
}
