namespace BusTracking.Web.Areas.SystemAdmin.Controllers
{
    [Area("SystemAdmin")]
    [Authorize(Roles = "SystemAdmin")]
    public class TimeZoneController : Controller
    {
        private readonly AppDbContext _db;

        public TimeZoneController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(string search, int page = 1)
        {
            var query = _db.TimeZoneMasters.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(t => t.TimeZoneName.Contains(s) || t.IanaTimeZoneId.Contains(s) || t.WindowsTimeZoneId.Contains(s));
            }

            int pageSize = 10;
            int totalItems = await query.CountAsync();
            var items = await query
                .OrderBy(t => t.DisplayOrder)
                .ThenBy(t => t.TimeZoneId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.TotalItems = totalItems;

            return View(items);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new TimeZoneMaster { IsActive = true, DisplayOrder = 1 });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TimeZoneMaster model)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.TimeZoneName = model.TimeZoneName.Trim();
            model.IanaTimeZoneId = model.IanaTimeZoneId.Trim();
            model.WindowsTimeZoneId = model.WindowsTimeZoneId.Trim();
            model.UtcOffset = model.UtcOffset.Trim();

            _db.TimeZoneMasters.Add(model);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Time Zone record created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _db.TimeZoneMasters.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TimeZoneMaster model)
        {
            if (id != model.TimeZoneId) return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var item = await _db.TimeZoneMasters.FindAsync(id);
            if (item == null) return NotFound();

            item.TimeZoneName = model.TimeZoneName.Trim();
            item.IanaTimeZoneId = model.IanaTimeZoneId.Trim();
            item.WindowsTimeZoneId = model.WindowsTimeZoneId.Trim();
            item.UtcOffset = model.UtcOffset.Trim();
            item.DisplayOrder = model.DisplayOrder;
            item.IsActive = model.IsActive;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Time Zone record updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.TimeZoneMasters.FindAsync(id);
            if (item == null) return NotFound();

            // Check if used by any school
            var isUsed = await _db.Schools.IgnoreQueryFilters().AnyAsync(s => s.TimeZoneId == id);
            if (isUsed)
            {
                TempData["ErrorMessage"] = "Cannot delete this Time Zone because it is currently assigned to one or more schools.";
                return RedirectToAction(nameof(Index));
            }

            _db.TimeZoneMasters.Remove(item);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Time Zone record deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var item = await _db.TimeZoneMasters.FindAsync(id);
            if (item == null) return NotFound();

            item.IsActive = !item.IsActive;
            await _db.SaveChangesAsync();

            return Json(new { success = true, isActive = item.IsActive });
        }
    }
}
