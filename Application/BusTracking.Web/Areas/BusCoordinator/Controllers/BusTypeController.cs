using BusTracking.Common.DTOs.Common;

namespace BusTracking.Web.Areas.BusCoordinator.Controllers
{
    [Area("BusCoordinator"), Authorize(Roles = "BusCoordinator")]
    public class BusTypeController : Controller
    {
        private readonly IBusTypeService _busType;
        public BusTypeController(IBusTypeService busType) => _busType = busType;

        // GET /BusCoordinator/BusType
        public async Task<IActionResult> Index([FromQuery] string? search = null, [FromQuery] int page = 1)
        {
            if (!PermissionHelper.Can(User, "bustype.view")) return Forbid();

            ViewBag.CanAdd = PermissionHelper.Can(User, "bustype.add");
            ViewBag.CanEdit = PermissionHelper.Can(User, "bustype.edit");
            ViewBag.CanDelete = PermissionHelper.Can(User, "bustype.delete");
            ViewBag.Search = search;

            var r = await _busType.GetAllAsync(search, page);
            return View(r.Data ?? new PagedResult<BusTypeDto>());
        }

        // POST /BusCoordinator/BusType/Create  (AJAX — row insert)
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] SaveBusTypeDto dto)
        {
            if (!PermissionHelper.Can(User, "bustype.add"))
                return Json(new { success = false, message = "Permission denied." });

            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Name is required." });

            var r = await _busType.CreateAsync(dto);
            if (!r.Success) return Json(new { success = false, message = r.Message });

            return Json(new
            {
                success = true,
                message = r.Message,
                id = r.Data?.Id,
                name = r.Data?.Name,
                busCount = r.Data?.BusCount ?? 0
            });
        }

        // POST /BusCoordinator/BusType/Edit/{id}  (AJAX — row update)
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromBody] SaveBusTypeDto dto)
        {
            if (!PermissionHelper.Can(User, "bustype.edit"))
                return Json(new { success = false, message = "Permission denied." });

            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Name is required." });

            var r = await _busType.UpdateAsync(id, dto);
            return Json(new { r.Success, r.Message, name = dto.Name.Trim() });
        }

        // POST /BusCoordinator/BusType/Delete/{id}  (AJAX)
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!PermissionHelper.Can(User, "bustype.delete"))
                return Json(new { success = false, message = "Permission denied." });

            var r = await _busType.DeleteAsync(id);
            return Json(new { r.Success, r.Message });
        }
    }
}