using BusTracking.Common.DTOs.Common;

namespace BusTracking.Web.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin"), Authorize(Roles = "SuperAdmin")]
    public class BusTypeController : Controller
    {
        private readonly IBusTypeService _busType;
        public BusTypeController(IBusTypeService busType) => _busType = busType;

        // GET /SuperAdmin/BusType
        public async Task<IActionResult> Index([FromQuery] string? search = null, [FromQuery] int page = 1)
        {
            ViewBag.Search = search;
            var r = await _busType.GetAllAsync(search, page);
            return View(r.Data ?? new PagedResult<BusTypeDto>());
        }

        // POST /SuperAdmin/BusType/Create  (AJAX — row insert)
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] SaveBusTypeDto dto)
        {
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

        // POST /SuperAdmin/BusType/Edit/{id}  (AJAX — row update)
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromBody] SaveBusTypeDto dto)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Name is required." });

            var r = await _busType.UpdateAsync(id, dto);
            return Json(new { r.Success, r.Message, name = dto.Name.Trim() });
        }

        // POST /SuperAdmin/BusType/Delete/{id}  (AJAX)
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var r = await _busType.DeleteAsync(id);
            return Json(new { r.Success, r.Message });
        }
    }
}
