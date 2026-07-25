namespace BusTracking.API.Controllers
{
    [Authorize, Route("api/[controller]")]
    public class FuelLogController : ApiBaseController
    {
        private readonly AppDbContext _db;
        public FuelLogController(AppDbContext db) { _db = db; }

        public class CreateFuelLogDto
        {
            public int BusId { get; set; }
            public decimal OdometerReading { get; set; }
            public decimal FuelLiters { get; set; }
            public decimal TotalCost { get; set; }
            public DateOnly FuelDate { get; set; }
            public string? Notes { get; set; }
        }

        public class UpdateFuelLogDto
        {
            public int BusId { get; set; }
            public decimal OdometerReading { get; set; }
            public decimal FuelLiters { get; set; }
            public decimal TotalCost { get; set; }
            public DateOnly FuelDate { get; set; }
            public string? Notes { get; set; }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? busId = null)
        {
            var query = _db.BusFuelLogs
                .Include(l => l.Bus)
                .Include(l => l.Driver)
                .AsQueryable();

            if (CurrentUserRole == "BusCoordinator")
            {
                var coord = await _db.Users.FirstOrDefaultAsync(u => u.UserId == CurrentUserId);
                if (coord != null && coord.SchoolId.HasValue)
                {
                    query = query.Where(l => l.Bus.SchoolId == coord.SchoolId.Value);
                }
            }

            if (busId.HasValue && busId.Value > 0)
                query = query.Where(l => l.BusId == busId.Value);

            var logs = await query
                .OrderByDescending(l => l.FuelDate)
                .ThenByDescending(l => l.CreatedAt)
                .Select(l => new
                {
                    l.FuelLogId,
                    l.BusId,
                    BusNumber = l.Bus != null ? l.Bus.BusNumber : "–",
                    BusName = l.Bus != null ? l.Bus.BusName : "–",
                    DriverName = l.Driver != null ? l.Driver.FullName : "–",
                    l.OdometerReading,
                    l.FuelLiters,
                    l.TotalCost,
                    FuelDate = l.FuelDate.ToString("yyyy-MM-dd"),
                    l.Notes
                })
                .ToListAsync();

            return Ok(ApiResponse<object>.Ok(logs));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFuelLogDto dto)
        {
            if (dto.BusId <= 0 || dto.FuelLiters <= 0)
                return BadRequest(ApiResponse<bool>.Fail("Valid BusId and FuelLiters are required."));

            var log = new BusFuelLog
            {
                BusId = dto.BusId,
                DriverId = CurrentUserId,
                OdometerReading = dto.OdometerReading,
                FuelLiters = dto.FuelLiters,
                TotalCost = dto.TotalCost,
                FuelDate = dto.FuelDate == default ? DateOnly.FromDateTime(DateTime.UtcNow) : dto.FuelDate,
                Notes = dto.Notes
            };

            _db.BusFuelLogs.Add(log);
            await _db.SaveChangesAsync();

            return Ok(ApiResponse<bool>.Ok(true, "Fuel log created successfully."));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateFuelLogDto dto)
        {
            var log = await _db.BusFuelLogs.FindAsync(id);
            if (log is null)
                return NotFound(ApiResponse<bool>.Fail("Fuel log not found."));

            if (dto.BusId > 0) log.BusId = dto.BusId;
            log.OdometerReading = dto.OdometerReading;
            log.FuelLiters = dto.FuelLiters;
            log.TotalCost = dto.TotalCost;
            if (dto.FuelDate != default) log.FuelDate = dto.FuelDate;
            log.Notes = dto.Notes;

            await _db.SaveChangesAsync();
            return Ok(ApiResponse<bool>.Ok(true, "Fuel log updated successfully."));
        }

        [HttpGet("bus/{busId}")]
        public async Task<IActionResult> GetByBus(int busId)
        {
            var logs = await _db.BusFuelLogs
                .Include(l => l.Driver)
                .Where(l => l.BusId == busId)
                .OrderByDescending(l => l.FuelDate)
                .Select(l => new
                {
                    l.FuelLogId,
                    l.BusId,
                    DriverName = l.Driver != null ? l.Driver.FullName : "–",
                    l.OdometerReading,
                    l.FuelLiters,
                    l.TotalCost,
                    l.FuelDate,
                    l.Notes
                })
                .ToListAsync();

            return Ok(ApiResponse<object>.Ok(logs));
        }
    }
}
