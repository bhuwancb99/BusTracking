namespace BusTracking.Web.Areas.BusCoordinator.Controllers
{
    [Area("BusCoordinator")]
    [Authorize(Roles = "BusCoordinator")]
    public class ReportController : Controller
    {
        private readonly AppDbContext _db;
        public ReportController(AppDbContext db) { _db = db; }

        public async Task<IActionResult> Index()
        {
            var totalBuses = await _db.Buses.CountAsync(b => b.IsActive);
            var totalTrips = await _db.BusTrips.CountAsync();
            var completedTrips = await _db.BusTrips.CountAsync(t => t.Status == TripStatus.Completed);
            var totalStudents = await _db.Students.CountAsync();

            ViewBag.TotalBuses = totalBuses;
            ViewBag.TotalTrips = totalTrips;
            ViewBag.CompletedTrips = completedTrips;
            ViewBag.TotalStudents = totalStudents;
            ViewBag.OnTimeRate = totalTrips == 0 ? 100 : Math.Round((double)completedTrips / totalTrips * 100, 1);

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ExportStudentRoster()
        {
            var students = await _db.Students
                .Include(s => s.User)
                .Include(s => s.Bus)
                .Include(s => s.Stop)
                .Include(s => s.Standard)
                .Where(s => s.User.IsActive)
                .OrderBy(s => s.User.FullName)
                .ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("StudentCode,FullName,Standard,BusNumber,StopName,TransportFeeStatus");

            foreach (var s in students)
            {
                csv.AppendLine($"\"{s.StudentCode}\",\"{s.User.FullName}\",\"{s.Standard?.StandardName}\",\"{s.Bus?.BusNumber}\",\"{s.Stop?.StopName}\",\"{s.TransportFeeStatus}\"");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"Student_Transport_Roster_{DateTime.UtcNow:yyyyMMdd}.csv");
        }

        [HttpGet]
        public async Task<IActionResult> ExportTripLogs()
        {
            var trips = await _db.BusTrips
                .Include(t => t.Bus)
                .Include(t => t.Driver)
                .Include(t => t.Route)
                .OrderByDescending(t => t.CreatedAt)
                .Take(500)
                .ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("TripId,Date,TripType,BusNumber,DriverName,RouteCode,Status,StartedAt,EndedAt");

            foreach (var t in trips)
            {
                csv.AppendLine($"{t.TripId},{t.TripDate},{t.TripType},\"{t.Bus?.BusNumber}\",\"{t.Driver?.FullName}\",\"{t.Route?.RouteCode}\",{t.Status},{t.StartedAt:g},{t.EndedAt:g}");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"Trip_Logs_{DateTime.UtcNow:yyyyMMdd}.csv");
        }
    }
}
