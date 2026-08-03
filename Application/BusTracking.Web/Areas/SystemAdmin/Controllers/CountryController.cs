namespace BusTracking.Web.Areas.SystemAdmin.Controllers
{
    [Area("SystemAdmin")]
    [Authorize(Roles = "SystemAdmin")]
    public class CountryController : Controller
    {
        private readonly AppDbContext _db;

        public CountryController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(string search, int page = 1, int pageSize = 10)
        {
            if (pageSize <= 0) pageSize = 10;

            var query = _db.CountryMasters.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(c => c.CountryName.Contains(s) || (c.ISO2 != null && c.ISO2.Contains(s)) || (c.CurrencyCode != null && c.CurrencyCode.Contains(s)));
            }

            int totalItems = await query.CountAsync();
            var items = await query
                .OrderBy(c => c.CountryName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new
                {
                    c.CountryId,
                    c.CountryName,
                    c.ISO2,
                    c.PhoneCode,
                    c.CurrencyCode,
                    c.CurrencySymbol,
                    c.IsActive,
                    RegionCount = _db.RegionMasters.Count(r => r.CountryId == c.CountryId)
                })
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.TotalItems = totalItems;

            return View(items);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CountryMaster { IsActive = true });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CountryMaster model)
        {
            ModelState.Remove(nameof(model.Regions));
            ModelState.Remove(nameof(model.Schools));

            if (string.IsNullOrWhiteSpace(model.CountryName))
                ModelState.AddModelError(nameof(model.CountryName), "Country Name is required.");

            if (await _db.CountryMasters.AnyAsync(c => c.CountryName.ToLower() == model.CountryName.Trim().ToLower()))
                ModelState.AddModelError(nameof(model.CountryName), $"Country '{model.CountryName}' already exists.");

            if (!ModelState.IsValid)
                return View(model);

            model.CountryName = model.CountryName.Trim();
            model.ISO2 = model.ISO2?.Trim().ToUpper();
            model.PhoneCode = model.PhoneCode?.Trim();
            model.CurrencyCode = model.CurrencyCode?.Trim().ToUpper();
            model.CurrencySymbol = model.CurrencySymbol?.Trim();
            model.CreatedAt = DateTime.UtcNow;
            model.UpdatedAt = DateTime.UtcNow;

            _db.CountryMasters.Add(model);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Country created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _db.CountryMasters.FindAsync(id);
            if (item == null) return NotFound();

            return View(item);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CountryMaster model)
        {
            if (id != model.CountryId) return BadRequest();

            ModelState.Remove(nameof(model.Regions));
            ModelState.Remove(nameof(model.Schools));

            if (string.IsNullOrWhiteSpace(model.CountryName))
                ModelState.AddModelError(nameof(model.CountryName), "Country Name is required.");

            if (await _db.CountryMasters.AnyAsync(c => c.CountryId != id && c.CountryName.ToLower() == model.CountryName.Trim().ToLower()))
                ModelState.AddModelError(nameof(model.CountryName), $"Country '{model.CountryName}' already exists.");

            if (!ModelState.IsValid)
                return View(model);

            var item = await _db.CountryMasters.FindAsync(id);
            if (item == null) return NotFound();

            item.CountryName = model.CountryName.Trim();
            item.ISO2 = model.ISO2?.Trim().ToUpper();
            item.PhoneCode = model.PhoneCode?.Trim();
            item.CurrencyCode = model.CurrencyCode?.Trim().ToUpper();
            item.CurrencySymbol = model.CurrencySymbol?.Trim();
            item.IsActive = model.IsActive;
            item.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Country updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.CountryMasters.FindAsync(id);
            if (item == null) return NotFound();

            // Check if used by any school or has regions
            var isUsedInSchools = await _db.Schools.IgnoreQueryFilters().AnyAsync(s => s.CountryId == id);
            if (isUsedInSchools)
            {
                TempData["ErrorMessage"] = "Cannot delete this country because it is currently assigned to one or more schools.";
                return RedirectToAction(nameof(Index));
            }

            var hasRegions = await _db.RegionMasters.AnyAsync(r => r.CountryId == id);
            if (hasRegions)
            {
                TempData["ErrorMessage"] = "Cannot delete this country because it contains existing regions. Delete or reassign regions first.";
                return RedirectToAction(nameof(Index));
            }

            _db.CountryMasters.Remove(item);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Country deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var item = await _db.CountryMasters.FindAsync(id);
            if (item == null) return NotFound();

            item.IsActive = !item.IsActive;
            item.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Json(new { success = true, isActive = item.IsActive });
        }
    }
}
