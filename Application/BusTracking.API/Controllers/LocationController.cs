namespace BusTracking.API.Controllers
{
    [Authorize(Roles = "Driver"), Route("api/[controller]")]
    public class LocationController : ApiBaseController
    {
        private readonly AppDbContext _db;
        public LocationController(AppDbContext db) => _db = db;

        public class GpsPingRequest
        {
            public int TripId { get; set; }
            public int BusId { get; set; }
            public decimal Latitude { get; set; }
            public decimal Longitude { get; set; }
            public decimal? Speed { get; set; }
            public decimal? Heading { get; set; }
        }

        /// <summary>Receive GPS ping from MAUI Driver app</summary>
        [HttpPost("ping")]
        public async Task<IActionResult> Ping([FromBody] GpsPingRequest req)
        {
            _db.BusLiveLocations.Add(new BusLiveLocation
            {
                TripId = req.TripId,
                BusId = req.BusId,
                Latitude = req.Latitude,
                Longitude = req.Longitude,
                Speed = req.Speed,
                Heading = req.Heading,
                RecordedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
            return Ok(ApiResponse<bool>.Ok(true));
        }

        /// <summary>Get latest bus location (Student/Parent view)</summary>
        [Authorize, HttpGet("{tripId}/latest")]
        public async Task<IActionResult> GetLatest(int tripId)
        {
            var loc = await _db.BusLiveLocations
                .Where(l => l.TripId == tripId)
                .OrderByDescending(l => l.RecordedAt)
                .Select(l => new BusLocationDto
                {
                    Latitude = l.Latitude,
                    Longitude = l.Longitude,
                    Speed = l.Speed,
                    Heading = l.Heading,
                    RecordedAt = l.RecordedAt
                })
                .FirstOrDefaultAsync();

            if (loc is null) return NotFound(ApiResponse<BusLocationDto>.Fail("No location data."));
            return Ok(ApiResponse<BusLocationDto>.Ok(loc));
        }
    }
}
