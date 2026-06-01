namespace BusTracking.Web.Areas.BusCoordinator.Controllers
{
    [Area("BusCoordinator"), Authorize(Roles = "BusCoordinator")]
    public class BusController : Controller
    {
        private readonly IBusService _bus;
        private readonly IRouteService _route;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        public BusController(IBusService b, IRouteService r)
        {
            _bus = b;
            _route = r;
        }

        public async Task<IActionResult> Index(int page = 1, string? search = null, string? status = "Active")
        {
            // Normalise: "Both" and null both mean no status filter for the service
            var normalised = (status == "Both" || string.IsNullOrEmpty(status)) ? null : status;
            ViewBag.Search = search;
            ViewBag.Status = status ?? "Active"; // keep selection stable; default to Active on first load
            var r0 = await _bus.GetAllAsync(page, 10, search, normalised);
            return View(r0.Data);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var r = await _bus.GetByIdAsync(id);
            return r.Success ? View(r.Data) : NotFound();
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadRoutes();
            return View(new CreateBusDto());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBusDto m)
        {
            if (!ModelState.IsValid)
            {
                await LoadRoutes();
                return View(m);
            }
            var r = await _bus.CreateAsync(m, UserId);
            if (!r.Success)
            {
                ModelState.AddModelError("", r.Message);
                await LoadRoutes();
                return View(m);
            }
            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var r = await _bus.GetByIdAsync(id);
            if (!r.Success)
                return NotFound();
            ViewBag.BusId = id;
            await LoadRoutes(r.Data!.RouteId);
            return View(new UpdateBusDto
            {
                BusName = r.Data.BusName,
                BusNumber = r.Data.BusNumber,
                RouteId = r.Data.RouteId,
                Capacity = r.Data.Capacity,
                IsActive = r.Data.IsActive
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateBusDto m)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.BusId = id;
                await LoadRoutes(m.RouteId);
                return View(m);
            }
            var r = await _bus.UpdateAsync(id, m);
            if (!r.Success)
            {
                ModelState.AddModelError("", r.Message);
                ViewBag.BusId = id;
                await LoadRoutes(m.RouteId);
                return View(m);
            }
            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _bus.DeleteAsync(id);
            TempData["SuccessMessage"] = "Marked inactive.";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadRoutes(int? selectedId = null)
        {
            var routes = await _route.GetAllAsync(1, 100, null);
            ViewBag.Routes = (routes.Data?.Items ?? [])
                .Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = r.RouteId.ToString(),
                    Text = $"{r.RouteName} ({r.RouteCode})",
                    Selected = r.RouteId == selectedId
                }).ToList();
        }
    }
}
