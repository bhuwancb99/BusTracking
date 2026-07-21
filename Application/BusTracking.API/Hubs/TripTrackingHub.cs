namespace BusTracking.API.Hubs
{
    /// <summary>
    /// SignalR hub for real-time bus location broadcasting.
    ///
    /// Groups:
    ///   "trip-{tripId}"  — all clients watching this trip (parents, coordinator, student)
    ///   "driver-{tripId}" — only the driver (for receiving admin messages later)
    ///
    /// Flow:
    ///   1. Driver app connects → calls JoinAsDriver(tripId)
    ///   2. Parent/Student app connects → calls WatchTrip(tripId)
    ///   3. Driver calls SendLocation(tripId, lat, lng, speed, heading) every ~5s
    ///   4. Hub broadcasts "BusLocationUpdated" to the trip group instantly
    ///   5. When trip ends, driver calls TripEnded(tripId) → notifies all watchers
    /// </summary>
    [Authorize]
    public class TripTrackingHub : Hub
    {
        private readonly AppDbContext _db;
        public TripTrackingHub(AppDbContext db) => _db = db;

        // ──────────────────────────────────────────────────────────────────
        // DRIVER — joins as broadcaster for this trip
        // ──────────────────────────────────────────────────────────────────
        public async Task JoinAsDriver(int tripId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"trip-{tripId}");
            await Groups.AddToGroupAsync(Context.ConnectionId, $"driver-{tripId}");
        }

        // ──────────────────────────────────────────────────────────────────
        // PARENT / STUDENT / COORDINATOR — join as watcher
        // ──────────────────────────────────────────────────────────────────
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
                await Clients.Caller.SendAsync("BusLocationUpdated",
                    last.Latitude, last.Longitude,
                    last.Speed, last.Heading, last.RecordedAt);
        }

        public async Task LeaveTrip(int tripId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"trip-{tripId}");
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"driver-{tripId}");
        }

        // ──────────────────────────────────────────────────────────────────
        // DRIVER sends location → hub saves + broadcasts
        // ──────────────────────────────────────────────────────────────────
        public async Task SendLocation(
            int tripId, int busId,
            decimal lat, decimal lng,
            decimal? speed, decimal? heading)
        {
            // Retrieve time zone for logged-in user / tenant
            var timeZoneInfoId = Context.User?.FindFirst("time_zone_id")?.Value
                              ?? Context.User?.FindFirst("TimeZoneInfoId")?.Value
                              ?? "India Standard Time";
            var now = TimeZoneHelper.GetNow(timeZoneInfoId);

            // Persist to DB (keeps history + supports late-joining clients)
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

            // Broadcast to everyone watching this trip instantly
            var timeStamp = now.ToString("o");
            await Clients.Group($"trip-{tripId}")
                .SendAsync("BusLocationUpdated", lat, lng, speed, heading, timeStamp);
        }

        // ──────────────────────────────────────────────────────────────────
        // DRIVER notifies when trip ends
        // ──────────────────────────────────────────────────────────────────
        public async Task TripEnded(int tripId)
        {
            await Clients.Group($"trip-{tripId}")
                .SendAsync("TripEnded", tripId);
        }
    }
}
