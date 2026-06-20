namespace BusTracking.Web.Areas.BusCoordinator.Controllers
{
    [Area("BusCoordinator"), Authorize(Roles = "BusCoordinator")]
    public class BusController : Controller
    {
        private readonly IBusService _bus;
        private readonly IRouteService _route;
        private readonly IBusTypeService _busType;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        public BusController(IBusService b, IRouteService r, IBusTypeService busType)
        {
            _bus = b;
            _route = r;
            _busType = busType;
        }

        public async Task<IActionResult> Index(int page = 1, string? search = null, string? status = "Active")
        {
            if (!PermissionHelper.Can(User, "bus.view")) return Forbid();
            var normalised = (status == "Both" || string.IsNullOrEmpty(status)) ? null : status;
            ViewBag.Search = search;
            ViewBag.Status = status ?? "Active";
            var r0 = await _bus.GetAllAsync(page, 10, search, normalised);
            return View(r0.Data);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (!PermissionHelper.Can(User, "bus.view")) return Forbid();
            var r = await _bus.GetByIdAsync(id);
            return r.Success ? View(r.Data) : NotFound();
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!PermissionHelper.Can(User, "bus.add")) return Forbid();
            await LoadRoutes();
            await LoadBusTypes();
            return View(new CreateBusDto());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBusDto m)
        {
            if (!PermissionHelper.Can(User, "bus.add")) return Forbid();
            if (!ModelState.IsValid)
            {
                await LoadRoutes();
                await LoadBusTypes(m.BusTypeId);
                return View(m);
            }
            var r = await _bus.CreateAsync(m, UserId);
            if (!r.Success)
            {
                ModelState.AddModelError("", r.Message);
                await LoadRoutes();
                await LoadBusTypes(m.BusTypeId);
                return View(m);
            }
            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!PermissionHelper.Can(User, "bus.edit")) return Forbid();
            var r = await _bus.GetByIdAsync(id);
            if (!r.Success) return NotFound();
            ViewBag.BusId = id;
            await LoadRoutes(r.Data!.RouteId);
            await LoadBusTypes(r.Data.BusTypeId);
            return View(new UpdateBusDto
            {
                BusName = r.Data.BusName,
                BusNumber = r.Data.BusNumber,
                RouteId = r.Data.RouteId,
                BusTypeId = r.Data.BusTypeId,
                Capacity = r.Data.Capacity,
                IsActive = r.Data.IsActive
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateBusDto m)
        {
            if (!PermissionHelper.Can(User, "bus.edit")) return Forbid();
            if (!ModelState.IsValid)
            {
                ViewBag.BusId = id;
                await LoadRoutes(m.RouteId);
                await LoadBusTypes(m.BusTypeId);
                return View(m);
            }
            var r = await _bus.UpdateAsync(id, m);
            if (!r.Success)
            {
                ModelState.AddModelError("", r.Message);
                ViewBag.BusId = id;
                await LoadRoutes(m.RouteId);
                await LoadBusTypes(m.BusTypeId);
                return View(m);
            }
            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!PermissionHelper.Can(User, "bus.delete")) return Forbid();
            await _bus.DeleteAsync(id);
            TempData["SuccessMessage"] = "Marked inactive.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int id)
        {
            if (!PermissionHelper.Can(User, "bus.edit"))
                return Json(new { Success = false, Message = "Permission denied." });
            var r = await _bus.ToggleActiveAsync(id);
            return Json(new { r.Success, r.Message });
        }

        private async Task LoadRoutes(int? selectedId = null)
        {
            var routes = await _route.GetDropdownAsync();
            ViewBag.Routes = routes
                .Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = r.RouteId.ToString(),
                    Text = $"{r.RouteName} ({r.RouteCode})",
                    Selected = r.RouteId == selectedId
                }).ToList();
        }

        private async Task LoadBusTypes(int? selectedId = null)
        {
            var types = await _busType.GetDropdownAsync();
            ViewBag.BusTypes = (types.Data ?? [])
                .Select(t => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name,
                    Selected = t.Id == selectedId
                }).ToList();
        }
    }
}
