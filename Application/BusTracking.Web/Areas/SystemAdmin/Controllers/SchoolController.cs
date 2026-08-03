namespace BusTracking.Web.Areas.SystemAdmin.Controllers
{
    [Area("SystemAdmin")]
    [Authorize(Roles = "SystemAdmin")]
    public class SchoolController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IImageService _img;

        public SchoolController(AppDbContext db, IImageService img)
        {
            _db = db;
            _img = img;
        }

        public async Task<IActionResult> Index(string search, int page = 1, int pageSize = 10)
        {
            if (pageSize <= 0) pageSize = 10;

            var query = _db.Schools
                .Include(s => s.TimeZone)
                .Include(s => s.Country)
                .Include(s => s.Region)
                .IgnoreQueryFilters()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(sc => sc.SchoolName.Contains(s) || sc.SchoolCode.Contains(s) || sc.PrincipalName.Contains(s) || (sc.City != null && sc.City.Contains(s)));
            }

            int totalItems = await query.CountAsync();
            var items = await query
                .OrderByDescending(sc => sc.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.TotalItems = totalItems;

            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsViewBagAsync();
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(School school, IFormFile? logoFile)
        {
            ModelState.Remove(nameof(school.Country));
            ModelState.Remove(nameof(school.Region));
            ModelState.Remove(nameof(school.TimeZone));
            ModelState.Remove(nameof(school.Users));

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsViewBagAsync(school.CountryId, school.RegionId);
                return View(school);
            }

            // Check unique School Code
            var exists = await _db.Schools.IgnoreQueryFilters().AnyAsync(s => s.SchoolCode == school.SchoolCode.Trim());
            if (exists)
            {
                ModelState.AddModelError("SchoolCode", "School Code already exists.");
                await PopulateDropdownsViewBagAsync(school.CountryId, school.RegionId);
                return View(school);
            }

            school.SchoolName = school.SchoolName.Trim();
            school.SchoolCode = school.SchoolCode.Trim();
            school.SchoolAddress = school.SchoolAddress.Trim();
            school.ContactNumber = school.ContactNumber.Trim();
            school.EmailAddress = school.EmailAddress.Trim();
            school.PrincipalName = school.PrincipalName.Trim();
            school.Website = school.Website?.Trim();
            school.City = school.City?.Trim();
            school.CreatedAt = DateTime.UtcNow;
            school.UpdatedAt = DateTime.UtcNow;

            if (school.TimeZoneId.HasValue && school.TimeZoneId.Value > 0)
            {
                var tzItem = await _db.TimeZoneMasters.FindAsync(school.TimeZoneId.Value);
                if (tzItem != null)
                {
                    school.TimeZoneInfoId = tzItem.WindowsTimeZoneId;
                }
            }

            _db.Schools.Add(school);
            await _db.SaveChangesAsync();

            // Logo Upload Handler
            if (logoFile != null && logoFile.Length > 0)
            {
                try
                {
                    var logoUrl = await _img.SaveSchoolLogoAsync(logoFile, school.SchoolId, null);
                    school.SchoolLogo = logoUrl;
                    await _db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "School created, but logo upload failed: " + ex.Message;
                }
            }

            // Copy default AppConfigurations from School #1 template if available
            var masterConfigs = await _db.AppConfigurations
                .IgnoreQueryFilters()
                .Where(c => c.SchoolId == 1)
                .ToListAsync();

            if (masterConfigs.Count > 0 && school.SchoolId != 1)
            {
                var newSchoolConfigs = masterConfigs.Select(c => new AppConfiguration
                {
                    SchoolId = school.SchoolId,
                    ConfigKey = c.ConfigKey,
                    ConfigValue = c.ConfigValue,
                    Description = c.Description,
                    Platform = c.Platform,
                    IsActive = c.IsActive,
                    CreatedBy = c.CreatedBy,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }).ToList();

                await _db.AppConfigurations.AddRangeAsync(newSchoolConfigs);
                await _db.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "School created successfully with location and default App Configurations.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var school = await _db.Schools.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.SchoolId == id);
            if (school == null) return NotFound();

            await PopulateDropdownsViewBagAsync(school.CountryId, school.RegionId);
            return View(school);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, School model, IFormFile? logoFile)
        {
            var school = await _db.Schools.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.SchoolId == id);
            if (school == null) return NotFound();

            ModelState.Remove(nameof(model.Country));
            ModelState.Remove(nameof(model.Region));
            ModelState.Remove(nameof(model.TimeZone));
            ModelState.Remove(nameof(model.Users));

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsViewBagAsync(model.CountryId, model.RegionId);
                return View(model);
            }

            // Check unique School Code
            var exists = await _db.Schools.IgnoreQueryFilters()
                .AnyAsync(s => s.SchoolCode == model.SchoolCode.Trim() && s.SchoolId != id);
            if (exists)
            {
                ModelState.AddModelError("SchoolCode", "School Code already exists.");
                await PopulateDropdownsViewBagAsync(model.CountryId, model.RegionId);
                return View(model);
            }

            if (logoFile != null && logoFile.Length > 0)
            {
                try
                {
                    var logoUrl = await _img.SaveSchoolLogoAsync(logoFile, school.SchoolId, school.SchoolLogo);
                    school.SchoolLogo = logoUrl;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Logo upload failed: " + ex.Message);
                    await PopulateDropdownsViewBagAsync(model.CountryId, model.RegionId);
                    return View(model);
                }
            }

            if (model.TimeZoneId.HasValue && model.TimeZoneId.Value > 0)
            {
                var tzItem = await _db.TimeZoneMasters.FindAsync(model.TimeZoneId.Value);
                if (tzItem != null)
                {
                    school.TimeZoneId = model.TimeZoneId;
                    school.TimeZoneInfoId = tzItem.WindowsTimeZoneId;
                }
            }
            else
            {
                school.TimeZoneId = null;
            }

            school.SchoolName = model.SchoolName.Trim();
            school.SchoolCode = model.SchoolCode.Trim();
            school.SchoolAddress = model.SchoolAddress.Trim();
            school.ContactNumber = model.ContactNumber.Trim();
            school.EmailAddress = model.EmailAddress.Trim();
            school.PrincipalName = model.PrincipalName.Trim();
            school.Website = model.Website?.Trim();
            school.CountryId = model.CountryId;
            school.RegionId = model.RegionId;
            school.City = model.City?.Trim();
            school.IsActive = model.IsActive;
            school.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "School updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetRegionsByCountry(int countryId)
        {
            var regions = await _db.RegionMasters.AsNoTracking()
                .Where(r => r.CountryId == countryId && r.IsActive)
                .OrderBy(r => r.RegionName)
                .Select(r => new { r.RegionId, r.RegionName, r.RegionCode })
                .ToListAsync();

            return Json(new { success = true, data = regions });
        }

        private async Task PopulateDropdownsViewBagAsync(int? countryId = null, int? regionId = null)
        {
            var timeZones = await _db.TimeZoneMasters.Where(t => t.IsActive).OrderBy(t => t.DisplayOrder).ToListAsync();
            ViewBag.TimeZones = timeZones.Select(t => new SelectListItem
            {
                Value = t.TimeZoneId.ToString(),
                Text = t.TimeZoneName
            }).ToList();

            var countries = await _db.CountryMasters.Where(c => c.IsActive).OrderBy(c => c.CountryName).ToListAsync();
            ViewBag.Countries = new SelectList(countries, "CountryId", "CountryName", countryId);

            if (countryId.HasValue && countryId.Value > 0)
            {
                var regions = await _db.RegionMasters
                    .Where(r => r.CountryId == countryId.Value && r.IsActive)
                    .OrderBy(r => r.RegionName)
                    .ToListAsync();
                ViewBag.Regions = new SelectList(regions, "RegionId", "RegionName", regionId);
            }
            else
            {
                ViewBag.Regions = new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var school = await _db.Schools.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.SchoolId == id);
            if (school == null) return NotFound();

            school.IsActive = !school.IsActive;
            school.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Json(new { success = true, isActive = school.IsActive });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportAppConfigs(int id)
        {
            var school = await _db.Schools.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.SchoolId == id);
            if (school == null) return NotFound();

            var masterConfigs = await _db.AppConfigurations
                .IgnoreQueryFilters()
                .Where(c => c.SchoolId == 1)
                .ToListAsync();

            if (masterConfigs.Count == 0)
            {
                TempData["ErrorMessage"] = "No master AppConfigurations found under School #1 to import.";
                return RedirectToAction(nameof(Edit), new { id });
            }

            var existingKeys = await _db.AppConfigurations
                .IgnoreQueryFilters()
                .Where(c => c.SchoolId == id)
                .Select(c => new { c.ConfigKey, c.Platform })
                .ToListAsync();

            var existingKeySet = existingKeys.Select(k => $"{k.ConfigKey}_{k.Platform}").ToHashSet();

            var missingConfigs = masterConfigs
                .Where(c => !existingKeySet.Contains($"{c.ConfigKey}_{c.Platform}"))
                .Select(c => new AppConfiguration
                {
                    SchoolId = id,
                    ConfigKey = c.ConfigKey,
                    ConfigValue = c.ConfigValue,
                    Description = c.Description,
                    Platform = c.Platform,
                    IsActive = c.IsActive,
                    CreatedBy = c.CreatedBy,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                })
                .ToList();

            if (missingConfigs.Count == 0)
            {
                TempData["SuccessMessage"] = "All master AppConfigurations already exist for this school. Nothing new to import.";
            }
            else
            {
                await _db.AppConfigurations.AddRangeAsync(missingConfigs);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Successfully imported {missingConfigs.Count} missing AppConfiguration record(s) from School #1.";
            }

            return RedirectToAction(nameof(Edit), new { id });
        }
    }
}
