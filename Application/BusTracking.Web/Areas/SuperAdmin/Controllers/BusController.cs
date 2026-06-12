namespace BusTracking.Web.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin"), Authorize(Roles = "SuperAdmin")]
    public class BusController : Controller
    {
        private readonly IBusService _bus;
        private readonly IDriverService _driver;
        private readonly IRouteService _route;
        private readonly IBusTypeService _busType;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        public BusController(IBusService bus, IDriverService driver, IRouteService route, IBusTypeService busType)
        {
            _bus = bus;
            _driver = driver;
            _route = route;
            _busType = busType;
        }

        public async Task<IActionResult> Index(int page = 1, string? search = null, string? status = "Active")
        {
            var normalised = (status == "Both" || string.IsNullOrEmpty(status)) ? null : status;
            ViewBag.Status = status;
            // Keep submitted value in ViewBag so radio stays selected
            return View(await _bus.GetAllAsync(page, 10, search, normalised).D());
        }

        public async Task<IActionResult> Details(int id)
        {
            var r = await _bus.GetByIdAsync(id);
            return r.Success ? View(r.Data) : NotFound();
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadRoutesDropdown();
            await LoadBusTypesDropdown();
            return View(new CreateBusDto());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBusDto m)
        {
            if (!ModelState.IsValid)
            {
                await LoadRoutesDropdown();
                await LoadBusTypesDropdown(m.BusTypeId);
                return View(m);
            }
            var r = await _bus.CreateAsync(m, UserId);
            if (!r.Success)
            {
                ModelState.AddModelError("", r.Message);
                await LoadRoutesDropdown();
                await LoadBusTypesDropdown(m.BusTypeId);
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
                return NotFound(); ViewBag.BusId = id;
            await LoadRoutesDropdown(r.Data!.RouteId);
            await LoadBusTypesDropdown(r.Data.BusTypeId);
            if (r.Data.DriverUserId.HasValue && r.Data.DriverName != null)
                ViewBag.DriverDisplay = $"{r.Data.DriverName} ({r.Data.DriverPhone ?? "–"})";
            return View(new UpdateBusDto
            {
                BusName = r.Data!.BusName,
                BusNumber = r.Data.BusNumber,
                RouteId = r.Data.RouteId,
                BusTypeId = r.Data.BusTypeId,
                Capacity = r.Data.Capacity,
                DriverUserId = r.Data.DriverUserId,
                IsActive = r.Data.IsActive
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateBusDto m)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.BusId = id;
                await LoadRoutesDropdown(m.RouteId);
                await LoadBusTypesDropdown(m.BusTypeId);
                return View(m);
            }
            var r = await _bus.UpdateAsync(id, m);
            if (!r.Success)
            {
                ModelState.AddModelError("", r.Message);
                ViewBag.BusId = id;
                await LoadRoutesDropdown(m.RouteId);
                await LoadBusTypesDropdown(m.BusTypeId);
                return View(m);
            }
            TempData["SuccessMessage"] = r.Message;
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadRoutesDropdown(int? selectedId = null)
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

        private async Task LoadBusTypesDropdown(int? selectedId = null)
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

        [HttpGet]
        public async Task<IActionResult> SearchRoutes(string? q)
        {
            var r = await _route.GetAllAsync(1, 20, q);
            var list = (r.Data?.Items ?? []).Select(x => new { routeId = x.RouteId, display = $"{x.RouteName} ({x.RouteCode})" });
            return Json(list);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _bus.DeleteAsync(id);
            TempData["SuccessMessage"] = "Marked inactive.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(int id)
        {
            var r = await _bus.ToggleActiveAsync(id);
            return Json(new { r.Success, r.Message });
        }

        [HttpPost]
        public async Task<IActionResult> AssignDriver([FromBody] AssignDriverToBusDto dto)
        {
            var r = await _bus.AssignDriverAsync(dto);
            return Json(new { r.Success, r.Message });
        }

        [HttpPost]
        public async Task<IActionResult> AssignStudent([FromBody] AssignStudentRequest req)
        {
            var r = await _bus.AssignStudentAsync(req.BusId, req.StudentId);
            return Json(new { r.Success, r.Message });
        }

        [HttpGet]
        public async Task<IActionResult> SearchDrivers(string? q)
        {
            var r = await _driver.GetDropdownAsync(q);
            return Json(r.Data);
        }
    }
    internal static class X
    {
        internal static async Task<T> D<T>(this Task<BusTracking.Common.DTOs.Common.ApiResponse<T>> t) => (await t).Data!;
    }
}