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
        /// Driver GPS ping → saves to BusLiveLocations DB table + broadcasts to SignalR group instantly.
        /// Called by DriverTrackingViewModel every 5 seconds during an active trip.
        /// </summary>
        [Authorize, HttpPost("ping")]
        public async Task<IActionResult> Ping([FromBody] GpsPingRequest req)
        {
            if (req == null || req.TripId <= 0)
                return BadRequest(ApiResponse<bool>.Fail("Valid TripId is required."));

            // If BusId is not passed, fetch it from the active BusTrip
            if (req.BusId <= 0)
            {
                var trip = await _db.BusTrips.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.TripId == req.TripId);
                if (trip != null) req.BusId = trip.BusId;
            }

            var timeZoneInfoId = User.FindFirst("time_zone_id")?.Value
                              ?? User.FindFirst("TimeZoneInfoId")?.Value
                              ?? "India Standard Time";
            var now = TimeZoneHelper.GetNow(timeZoneInfoId);

            // 1. Persist to BusLiveLocations table
            _db.BusLiveLocations.Add(new BusLiveLocation
            {
                TripId = req.TripId,
                BusId = req.BusId,
                Latitude = req.Latitude,
                Longitude = req.Longitude,
                Speed = req.Speed,
                Heading = req.Heading,
                RecordedAt = now
            });
            await _db.SaveChangesAsync();

            // 2. Real-time broadcast to everyone watching this trip
            var timeStamp = now.ToString("o");
            await _hub.Clients.Group($"trip-{req.TripId}")
                .SendAsync("BusLocationUpdated",
                    req.Latitude, req.Longitude,
                    req.Speed, req.Heading,
                    timeStamp);

            return Ok(ApiResponse<bool>.Ok(true));
        }

        /// <summary>
        /// GET /api/location/{tripId}/latest
        /// Returns the last known location from BusLiveLocations for late-joining clients.
        /// </summary>
        [Authorize, HttpGet("{tripId}/latest")]
        public async Task<IActionResult> GetLatest(int tripId)
        {
            var loc = await _db.BusLiveLocations
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Where(l => l.TripId == tripId)
                .OrderByDescending(l => l.RecordedAt)
                .Select(l => new BusLocationDto
                {
                    Latitude = l.Latitude,
                    Longitude = l.Longitude,
                    Speed = l.Speed,
                    Heading = l.Heading,
                    RecordedAt = l.RecordedAt
                }).FirstOrDefaultAsync();

            return Ok(ApiResponse<BusLocationDto?>.Ok(loc));
        }

        /// <summary>
        /// GET /api/location/{tripId}/history
        /// Returns full location trail from BusLiveLocations for drawing past route.
        /// </summary>
        [Authorize, HttpGet("{tripId}/history")]
        public async Task<IActionResult> GetHistory(int tripId)
        {
            var history = await _db.BusLiveLocations
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Where(l => l.TripId == tripId)
                .OrderBy(l => l.RecordedAt)
                .Select(l => new BusLocationDto
                {
                    Latitude = l.Latitude,
                    Longitude = l.Longitude,
                    Speed = l.Speed,
                    Heading = l.Heading,
                    RecordedAt = l.RecordedAt
                }).ToListAsync();

            return Ok(ApiResponse<List<BusLocationDto>>.Ok(history));
        }
    }
}
