namespace BusTracking.Web.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class FuelLogController : Controller
    {
        private readonly AppDbContext _db;
        public FuelLogController(AppDbContext db) { _db = db; }

        public async Task<IActionResult> Index(int? busId, int page = 1)
        {
            var query = _db.BusFuelLogs
                .Include(f => f.Bus)
                .Include(f => f.Driver)
                .AsQueryable();

            if (busId.HasValue && busId.Value > 0)
                query = query.Where(f => f.BusId == busId.Value);

            int pageSize = 10;
            int totalItems = await query.CountAsync();
            var items = await query
                .OrderByDescending(f => f.FuelDate)
                .ThenByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Buses = await _db.Buses.Where(b => b.IsActive).ToListAsync();
            ViewBag.SelectedBusId = busId;

            var pagedResult = new PagedResult<BusFuelLog>
            {
                Items = items,
                TotalCount = totalItems,
                PageNumber = page,
                PageSize = pageSize
            };

            return View(pagedResult);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BusFuelLog log)
        {
            if (log.BusId <= 0 || log.FuelLiters <= 0)
            {
                TempData["ErrorMessage"] = "Valid Bus and Fuel Liters are required.";
                return RedirectToAction(nameof(Index));
            }

            _db.BusFuelLogs.Add(log);
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Fuel log recorded successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BusFuelLog log)
        {
            if (log.FuelLogId <= 0 || log.BusId <= 0 || log.FuelLiters <= 0)
            {
                TempData["ErrorMessage"] = "Valid Fuel Log ID, Bus, and Fuel Liters are required.";
                return RedirectToAction(nameof(Index));
            }

            var existing = await _db.BusFuelLogs.FindAsync(log.FuelLogId);
            if (existing is null)
            {
                TempData["ErrorMessage"] = "Fuel log not found.";
                return RedirectToAction(nameof(Index));
            }

            existing.BusId = log.BusId;
            existing.OdometerReading = log.OdometerReading;
            existing.FuelLiters = log.FuelLiters;
            existing.TotalCost = log.TotalCost;
            existing.FuelDate = log.FuelDate;
            existing.Notes = log.Notes;

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Fuel log updated successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
