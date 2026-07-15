namespace BusTracking.Web.Areas.BusCoordinator.Controllers
{
    [Area("BusCoordinator")]
    [Authorize(Roles = "BusCoordinator")]
    public class LoggerController : Controller
    {
        private readonly AppDbContext _db;

        public LoggerController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(
            string? platform,
            string? search,
            DateTime? fromDate,
            DateTime? toDate,
            int page = 1,
            string sortBy = "Timestamp",
            string sortOrder = "desc")
        {
            if (!PermissionHelper.Can(User, "logs.view")) return Forbid();

            var today = DateTime.Today;
            if (fromDate == null) fromDate = today;
            if (toDate == null) toDate = today;

            var utcFrom = DateTime.SpecifyKind(fromDate.Value.Date, DateTimeKind.Local).ToUniversalTime();
            var utcTo = DateTime.SpecifyKind(toDate.Value.Date.AddDays(1).AddSeconds(-1), DateTimeKind.Local).ToUniversalTime();

            var query = _db.Loggers.AsQueryable();

            query = query.Where(l => l.Timestamp >= utcFrom && l.Timestamp <= utcTo);

            if (!string.IsNullOrEmpty(platform) && platform != "All")
            {
                query = query.Where(l => l.Platform == platform);
            }

            if (!string.IsNullOrEmpty(search))
            {
                var s = search.Trim();
                query = query.Where(l =>
                    (l.ExceptionMessage != null && l.ExceptionMessage.Contains(s)) ||
                    (l.StackTrace != null && l.StackTrace.Contains(s)) ||
                    (l.Username != null && l.Username.Contains(s)) ||
                    (l.ModuleName != null && l.ModuleName.Contains(s)) ||
                    (l.ActionName != null && l.ActionName.Contains(s)) ||
                    (l.AdditionalDetails != null && l.AdditionalDetails.Contains(s))
                );
            }

            bool isDesc = sortOrder.ToLower() == "desc";
            query = sortBy.ToLower() switch
            {
                "logid" => isDesc ? query.OrderByDescending(l => l.LogId) : query.OrderBy(l => l.LogId),
                "platform" => isDesc ? query.OrderByDescending(l => l.Platform) : query.OrderBy(l => l.Platform),
                "username" => isDesc ? query.OrderByDescending(l => l.Username) : query.OrderBy(l => l.Username),
                "role" => isDesc ? query.OrderByDescending(l => l.Role) : query.OrderBy(l => l.Role),
                "module" => isDesc ? query.OrderByDescending(l => l.ModuleName) : query.OrderBy(l => l.ModuleName),
                _ => isDesc ? query.OrderByDescending(l => l.Timestamp) : query.OrderBy(l => l.Timestamp)
            };

            int pageSize = 20;
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Platform = platform ?? "All";
            ViewBag.Search = search;
            ViewBag.FromDate = fromDate.Value.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate.Value.ToString("yyyy-MM-dd");
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;

            return View(items);
        }
    }
}
