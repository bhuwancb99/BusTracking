namespace BusTracking.Mobile.Services
{
    public class DriverTripService : IDriverTripService
    {
        private readonly IApiService _api;

        public DriverTripService(IApiService api) => _api = api;

        // ── Dashboard ─────────────────────────────────────────────────────
        // NOTE: No /api/driver/dashboard endpoint exists in the API.
        // DriverDashboardPage should derive its data from GetMyTripsAsync instead,
        // or a dedicated endpoint needs to be added to the API.
        public async Task<DriverDashboardData?> GetDashboardAsync()
        {
            // Fallback: build summary from today's trip
            var trips = await GetMyTripsAsync();
            var today = trips.FirstOrDefault();
            if (today is null) return null;
            return new DriverDashboardData
            {
                TripId = today.TripId,
                RouteName = today.RouteName,
                BusName = today.BusName,
                Status = today.Status
            };
        }

        // ── Trips  →  TripsController  [Route("api/[controller]")] ───────

        /// <summary>GET /api/trips/my-trip — returns today's assigned trip</summary>
        public async Task<List<DriverTripItem>> GetMyTripsAsync(string? date = null)
        {
            // The real API returns a single trip object, not a list.
            // We wrap it so callers don't need to change.
            var url = Constants.Driver.Trips;  // /api/trips/my-trip
            if (date != null) url += $"?date={Uri.EscapeDataString(date)}";
            var r = await _api.GetAsync<DriverTripItem>(url);
            return r.Data is not null ? [r.Data] : [];
        }

        /// <summary>GET /api/trips/{tripId}/stops</summary>
        public async Task<List<DriverTripStop>> GetTripStopsAsync(int tripId)
        {
            var r = await _api.GetAsync<List<DriverTripStop>>(
                string.Format(Constants.Driver.TripStops, tripId));
            return r.Data ?? [];
        }

        /// <summary>POST /api/trips/{tripId}/start</summary>
        public Task<ApiResponse<object>> StartTripAsync(int tripId)
            => _api.PostAsync<object>(string.Format(Constants.Driver.TripStart, tripId));

        /// <summary>POST /api/trips/{tripId}/end</summary>
        public Task<ApiResponse<object>> EndTripAsync(int tripId)
            => _api.PostAsync<object>(string.Format(Constants.Driver.TripEnd, tripId));

        /// <summary>
        /// No cancel endpoint exists in the driver API — only Admin/Coordinator can cancel.
        /// Returns a graceful failure so the UI can show an appropriate message.
        /// </summary>
        public Task<ApiResponse<object>> CancelTripAsync(int tripId)
            => Task.FromResult(ApiResponse<object>.Fail("Trip cancellation must be done by a coordinator."));

        // ── Boarding  →  BoardingController  [Route("api/trips/{tripId}/boarding")] ─

        /// <summary>PUT /api/trips/{tripId}/boarding</summary>
        public Task<ApiResponse<object>> UpdateBoardingAsync(int tripId, UpdateBoardingRequest req)
            => _api.PutAsync<object>(string.Format(Constants.Driver.TripBoarding, tripId), req);

        // ── Location  →  LocationController  [Route("api/[controller]")] ──

        /// <summary>POST /api/location/ping</summary>
        public Task<ApiResponse<object>> PingLocationAsync(int tripId, LocationPingRequest req)
            => _api.PostAsync<object>(Constants.Driver.LocationPing, req);
    }
}