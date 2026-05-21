namespace BusTracking.Web.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin"), Authorize(Roles = "SuperAdmin")]
    public class RouteController : Controller
    {
        private readonly IRouteService _route;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        public RouteController(IRouteService r) => _route = r;

        public async Task<IActionResult> Index(int page = 1, string? search = null)
        {
            ViewBag.Search = search;
            return View(await _route.GetAllAsync(page, 10, search));
        }
        public async Task<IActionResult> Details(int id)
        {
            var r = await _route.GetByIdAsync(id);
            return r.Success ? View(r.Data) : NotFound();
        }

        [HttpGet] public IActionResult Create() => View(new CreateRouteDto());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRouteDto m)
        {
            if (!ModelState.IsValid)
                return View(m);
            var r = await _route.CreateAsync(m, UserId);
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
            var r = await _route.GetByIdAsync(id);
            if (!r.Success)
                return NotFound();
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
            if (!ModelState.IsValid)
            {
                ViewBag.RouteId = id;
                return View(m);
            }
            var r = await _route.UpdateAsync(id, m);
            if (!r.Success)
            {
                ModelState.AddModelError("", r.Message);
                ViewBag.RouteId = id;
                return View(m);
            }
            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _route.DeleteAsync(id);
            TempData["SuccessMessage"] = "Deleted.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AddStop([FromBody] CreateStopDto dto)
        {
            var r = await _route.AddStopAsync(dto);
            return Json(new { r.Success, r.Message });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStop(int stopId)
        {
            var r = await _route.DeleteStopAsync(stopId);
            return Json(new { r.Success, r.Message });
        }
    }
}
