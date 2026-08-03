namespace BusTracking.Web.Areas.SystemAdmin.Controllers
{
    [Area("SystemAdmin")]
    [Authorize(Roles = "SystemAdmin")]
    public class RegionController : Controller
    {
        private readonly AppDbContext _db;

        public RegionController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(int? countryId, string search, int page = 1, int pageSize = 10)
        {
            if (pageSize <= 0) pageSize = 10;

            var query = _db.RegionMasters.Include(r => r.Country).AsNoTracking();

            if (countryId.HasValue && countryId.Value > 0)
            {
                query = query.Where(r => r.CountryId == countryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(r => r.RegionName.Contains(s) || (r.RegionCode != null && r.RegionCode.Contains(s)) || (r.Country != null && r.Country.CountryName.Contains(s)));
            }

            int totalItems = await query.CountAsync();
            var items = await query
                .OrderBy(r => r.Country != null ? r.Country.CountryName : string.Empty)
                .ThenBy(r => r.RegionName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new
                {
                    r.RegionId,
                    r.CountryId,
                    CountryName = r.Country != null ? r.Country.CountryName : string.Empty,
                    r.RegionName,
                    r.RegionCode,
                    r.IsActive,
                    r.CreatedAt
                })
                .ToListAsync();

            var countries = await _db.CountryMasters.AsNoTracking().OrderBy(c => c.CountryName).ToListAsync();
            ViewBag.Countries = new SelectList(countries, "CountryId", "CountryName", countryId);
            ViewBag.SelectedCountryId = countryId;
            ViewBag.Search = search;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.TotalItems = totalItems;

            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? countryId)
        {
            var countries = await _db.CountryMasters.AsNoTracking().Where(c => c.IsActive).OrderBy(c => c.CountryName).ToListAsync();
            ViewBag.Countries = new SelectList(countries, "CountryId", "CountryName", countryId);

            return View(new RegionMaster
            {
                CountryId = countryId ?? (countries.FirstOrDefault()?.CountryId ?? 0),
                IsActive = true
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RegionMaster model)
        {
            ModelState.Remove(nameof(model.Country));
            ModelState.Remove(nameof(model.Schools));

            if (model.CountryId <= 0)
                ModelState.AddModelError(nameof(model.CountryId), "Please select a valid country.");

            if (string.IsNullOrWhiteSpace(model.RegionName))
                ModelState.AddModelError(nameof(model.RegionName), "Region Name is required.");

            if (model.CountryId > 0 && !string.IsNullOrWhiteSpace(model.RegionName))
            {
                if (await _db.RegionMasters.AnyAsync(r => r.CountryId == model.CountryId && r.RegionName.ToLower() == model.RegionName.Trim().ToLower()))
                    ModelState.AddModelError(nameof(model.RegionName), $"Region '{model.RegionName}' already exists in the selected country.");
            }

            if (!ModelState.IsValid)
            {
                var countries = await _db.CountryMasters.AsNoTracking().Where(c => c.IsActive).OrderBy(c => c.CountryName).ToListAsync();
                ViewBag.Countries = new SelectList(countries, "CountryId", "CountryName", model.CountryId);
                return View(model);
            }

            model.RegionName = model.RegionName.Trim();
            model.RegionCode = model.RegionCode?.Trim().ToUpper();
            model.CreatedAt = DateTime.UtcNow;
            model.UpdatedAt = DateTime.UtcNow;

            _db.RegionMasters.Add(model);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Region created successfully.";
            return RedirectToAction(nameof(Index), new { countryId = model.CountryId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _db.RegionMasters.FindAsync(id);
            if (item == null) return NotFound();

            var countries = await _db.CountryMasters.AsNoTracking().OrderBy(c => c.CountryName).ToListAsync();
            ViewBag.Countries = new SelectList(countries, "CountryId", "CountryName", item.CountryId);

            return View(item);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RegionMaster model)
        {
            if (id != model.RegionId) return BadRequest();

            ModelState.Remove(nameof(model.Country));
            ModelState.Remove(nameof(model.Schools));

            if (model.CountryId <= 0)
                ModelState.AddModelError(nameof(model.CountryId), "Please select a valid country.");

            if (string.IsNullOrWhiteSpace(model.RegionName))
                ModelState.AddModelError(nameof(model.RegionName), "Region Name is required.");

            if (model.CountryId > 0 && !string.IsNullOrWhiteSpace(model.RegionName))
            {
                if (await _db.RegionMasters.AnyAsync(r => r.RegionId != id && r.CountryId == model.CountryId && r.RegionName.ToLower() == model.RegionName.Trim().ToLower()))
                    ModelState.AddModelError(nameof(model.RegionName), $"Region '{model.RegionName}' already exists in the selected country.");
            }

            if (!ModelState.IsValid)
            {
                var countries = await _db.CountryMasters.AsNoTracking().OrderBy(c => c.CountryName).ToListAsync();
                ViewBag.Countries = new SelectList(countries, "CountryId", "CountryName", model.CountryId);
                return View(model);
            }

            var item = await _db.RegionMasters.FindAsync(id);
            if (item == null) return NotFound();

            item.CountryId = model.CountryId;
            item.RegionName = model.RegionName.Trim();
            item.RegionCode = model.RegionCode?.Trim().ToUpper();
            item.IsActive = model.IsActive;
            item.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Region updated successfully.";
            return RedirectToAction(nameof(Index), new { countryId = model.CountryId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.RegionMasters.FindAsync(id);
            if (item == null) return NotFound();

            int countryId = item.CountryId;

            // Check if used by any school
            var isUsed = await _db.Schools.IgnoreQueryFilters().AnyAsync(s => s.RegionId == id);
            if (isUsed)
            {
                TempData["ErrorMessage"] = "Cannot delete this region because it is currently assigned to one or more schools.";
                return RedirectToAction(nameof(Index), new { countryId });
            }

            _db.RegionMasters.Remove(item);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Region deleted successfully.";
            return RedirectToAction(nameof(Index), new { countryId });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var item = await _db.RegionMasters.FindAsync(id);
            if (item == null) return NotFound();

            item.IsActive = !item.IsActive;
            item.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Json(new { success = true, isActive = item.IsActive });
        }
    }
}
