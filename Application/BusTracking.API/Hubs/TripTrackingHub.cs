namespace BusTracking.API.Hubs
{
    /// <summary>
    /// SignalR hub for real-time bus location broadcasting.
    ///
    /// Groups:
    ///   "trip-{tripId}"  — all clients watching this trip (parents, coordinator, student, admin)
    ///   "driver-{tripId}" — only the driver
    /// </summary>
    public class TripTrackingHub : Hub
    {
        private readonly AppDbContext _db;
        public TripTrackingHub(AppDbContext db) => _db = db;

        // ──────────────────────────────────────────────────────────────────
        // DRIVER — joins as broadcaster for this trip
        // ──────────────────────────────────────────────────────────────────
        [Authorize]
        public async Task JoinAsDriver(int tripId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"trip-{tripId}");
            await Groups.AddToGroupAsync(Context.ConnectionId, $"driver-{tripId}");
        }

        // ──────────────────────────────────────────────────────────────────
        // PARENT / STUDENT / COORDINATOR / SUPERADMIN — join as watcher
        // ──────────────────────────────────────────────────────────────────
        [AllowAnonymous]
        public async Task WatchTrip(int tripId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"trip-{tripId}");

            // Send the last known location immediately so the map shows
            // the bus pin right away, without waiting for the next ping
            var last = await _db.BusLiveLocations
                .Where(l => l.TripId == tripId)
                .OrderByDescending(l => l.RecordedAt)
                .Select(l => new
                {
                    l.Latitude,
                    l.Longitude,
                    l.Speed,
                    l.Heading,
                    RecordedAt = l.RecordedAt.ToString("o")
                })
                .FirstOrDefaultAsync();

            if (last is not null)
            {
                await Clients.Caller.SendAsync("BusLocationUpdated",
                    last.Latitude, last.Longitude,
                    last.Speed, last.Heading, last.RecordedAt);
            }
        }

        [AllowAnonymous]
        public async Task LeaveTrip(int tripId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"trip-{tripId}");
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"driver-{tripId}");
        }

        // ──────────────────────────────────────────────────────────────────
        // DRIVER sends location → hub saves + broadcasts
        // ──────────────────────────────────────────────────────────────────
        [Authorize]
        public async Task SendLocation(
            int tripId, int busId,
            decimal lat, decimal lng,
            decimal? speed, decimal? heading)
        {
            var timeZoneInfoId = Context.User?.FindFirst("time_zone_id")?.Value
                              ?? Context.User?.FindFirst("TimeZoneInfoId")?.Value
                              ?? "India Standard Time";
            var now = TimeZoneHelper.GetNow(timeZoneInfoId);

            _db.BusLiveLocations.Add(new BusLiveLocation
            {
                TripId = tripId,
                BusId = busId,
                Latitude = lat,
                Longitude = lng,
                Speed = speed,
                Heading = heading,
                RecordedAt = now
            });
            await _db.SaveChangesAsync();

            var timeStamp = now.ToString("o");
            await Clients.Group($"trip-{tripId}")
                .SendAsync("BusLocationUpdated", lat, lng, speed, heading, timeStamp);
        }

        // ──────────────────────────────────────────────────────────────────
        // DRIVER notifies when trip ends
        // ──────────────────────────────────────────────────────────────────
        [Authorize]
        public async Task TripEnded(int tripId)
        {
            await Clients.Group($"trip-{tripId}")
                .SendAsync("TripEnded", tripId);
        }
    }
}
