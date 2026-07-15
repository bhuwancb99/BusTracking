namespace BusTracking.API.Controllers
{
    [Authorize, Route("api/[controller]")]
    public class LocationController : ApiBaseController
    {
        private readonly AppDbContext _db;
        private readonly IHubContext<TripTrackingHub> _hub;

        public LocationController(AppDbContext db, IHubContext<TripTrackingHub> hub)
        {
            _db = db;
            _hub = hub;
        }

        /// <summary>
        /// POST /api/location/ping
        /// Driver GPS ping → saves to DB + broadcasts to SignalR group instantly.
        /// Called by DriverTrackingViewModel every 5 seconds during an active trip.
        /// </summary>
        [Authorize(Roles = "Driver"), HttpPost("ping")]
        public async Task<IActionResult> Ping([FromBody] GpsPingRequest req)
        {
            // 1. Persist
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

            // 2. Real-time broadcast to everyone watching this trip
            await _hub.Clients.Group($"trip-{req.TripId}")
                .SendAsync("BusLocationUpdated",
                    req.Latitude, req.Longitude,
                    req.Speed, req.Heading,
                    DateTime.UtcNow.ToString("o"));

            return Ok(ApiResponse<bool>.Ok(true));
        }

        /// <summary>
        /// GET /api/location/{tripId}/latest
        /// Returns the last known location for late-joining clients.
        /// </summary>
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

            if (loc is null)
                return NotFound(ApiResponse<BusLocationDto>.Fail("No location data yet."));

            return Ok(ApiResponse<BusLocationDto>.Ok(loc));
        }

        /// <summary>
        /// GET /api/location/{tripId}/history
        /// Full polyline path for the route replay view.
        /// </summary>
        [Authorize, HttpGet("{tripId}/history")]
        public async Task<IActionResult> GetHistory(int tripId)
        {
            var history = await _db.BusLiveLocations
                .Where(l => l.TripId == tripId)
                .OrderBy(l => l.RecordedAt)
                .Select(l => new { l.Latitude, l.Longitude, l.Speed, l.RecordedAt })
                .ToListAsync();

            return Ok(ApiResponse<object>.Ok(history));
        }
    }
}
