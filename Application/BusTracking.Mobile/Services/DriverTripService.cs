namespace BusTracking.Mobile.Services
{
    public class DriverTripService : IDriverTripService
    {
        private readonly IApiService _api;

        public DriverTripService(IApiService api) => _api = api;

        // ── Dashboard ─────────────────────────────────────────────────────
        public async Task<DriverDashboardData?> GetDashboardAsync()
        {
            var r = await _api.GetAsync<DriverDashboardData>(Constants.Driver.Dashboard);
            return r.Data;
        }

        // ── Trips ─────────────────────────────────────────────────────────
        public async Task<List<DriverTripItem>> GetMyTripsAsync(string? date = null)
        {
            var url = Constants.Driver.Trips;
            if (date != null) url += $"?date={Uri.EscapeDataString(date)}";
            var r = await _api.GetAsync<List<DriverTripItem>>(url);
            return r.Data ?? [];
        }

        public async Task<List<DriverTripStop>> GetTripStopsAsync(int tripId)
        {
            var r = await _api.GetAsync<List<DriverTripStop>>(
                string.Format(Constants.Driver.TripStops, tripId));
            return r.Data ?? [];
        }

        public Task<ApiResponse<object>> StartTripAsync(int tripId)
            => _api.PostAsync<object>(string.Format(Constants.Driver.TripStart, tripId));

        public Task<ApiResponse<object>> EndTripAsync(int tripId)
            => _api.PostAsync<object>(string.Format(Constants.Driver.TripEnd, tripId));

        public Task<ApiResponse<object>> CancelTripAsync(int tripId)
            => _api.PostAsync<object>(string.Format(Constants.Driver.TripCancel, tripId));

        // ── Boarding ──────────────────────────────────────────────────────
        public Task<ApiResponse<object>> UpdateBoardingAsync(int tripId, UpdateBoardingRequest req)
            => _api.PostAsync<object>(string.Format(Constants.Driver.TripBoarding, tripId), req);

        // ── Location ──────────────────────────────────────────────────────
        public Task<ApiResponse<object>> PingLocationAsync(int tripId, LocationPingRequest req)
            => _api.PostAsync<object>(string.Format(Constants.Driver.TripLocation, tripId), req);
    }
}