namespace BusTracking.Mobile.Services
{
    public class DriverTripService : IDriverTripService
    {
        private readonly IApiService _api;

        public DriverTripService(IApiService api) => _api = api;

        // ── Dashboard ─────────────────────────────────────────────────────
        // GET /api/trips/my-trip returns { Bus:{...}, Route:{...}, Trip:{...} }
        // We map this directly into DriverDashboardData.
        public async Task<DriverDashboardData?> GetDashboardAsync()
        {
            var r = await _api.GetAsync<MyTripResponse>(Constants.Driver.Trips); // /api/trips/my-trip
            if (r.Data is null) return null;

            var resp = r.Data;
            var dashboard = new DriverDashboardData
            {
                BusName = resp.Bus?.BusName ?? "",
                BusNumber = resp.Bus?.BusNumber ?? "",
                RouteName = resp.Route?.RouteName ?? "",
                TotalStudents = 0  // not returned by my-trip; set to 0 or fetch separately
            };

            if (resp.Trip is not null)
            {
                dashboard.ActiveTrip = new DriverTripItem
                {
                    TripId = resp.Trip.TripId,
                    BusName = dashboard.BusName,
                    BusNumber = dashboard.BusNumber,
                    RouteName = dashboard.RouteName,
                    TripType = resp.Trip.TripType ?? "",
                    Status = resp.Trip.Status ?? "",
                    StartedAt = resp.Trip.StartedAt,
                    EndedAt = resp.Trip.EndedAt
                };
            }

            return dashboard;
        }

        // ── Trips  →  TripsController  [Route("api/[controller]")] ───────

        /// <summary>GET /api/trips/my-trip — returns today's assigned trip</summary>
        public async Task<List<DriverTripItem>> GetMyTripsAsync(string? date = null)
        {
            // The real API returns a single nested object, not a list.
            // We wrap it so callers don't need to change.
            var url = Constants.Driver.Trips;  // /api/trips/my-trip
            if (date != null) url += $"?date={Uri.EscapeDataString(date)}";
            var r = await _api.GetAsync<MyTripResponse>(url);
            if (r.Data?.Trip is null) return [];

            var t = r.Data.Trip;
            return [new DriverTripItem
            {
                TripId    = t.TripId,
                BusName   = r.Data.Bus?.BusName   ?? "",
                BusNumber = r.Data.Bus?.BusNumber  ?? "",
                RouteName = r.Data.Route?.RouteName ?? "",
                TripType  = t.TripType  ?? "",
                Status    = t.Status    ?? "",
                StartedAt = t.StartedAt,
                EndedAt   = t.EndedAt
            }];
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

        /// <summary>GET /api/trips/{tripId}/students</summary>
        public async Task<List<DriverStudentStatus>> GetTripStudentsAsync(int tripId)
        {
            var r = await _api.GetAsync<List<DriverStudentStatus>>($"api/trips/{tripId}/students");
            return r.Data ?? [];
        }

        /// <summary>POST /api/trips/{tripId}/stops/{stopId}/reach</summary>
        public Task<ApiResponse<object>> ReachStopAsync(int tripId, int stopId)
            => _api.PostAsync<object>($"api/trips/{tripId}/stops/{stopId}/reach");

        /// <summary>POST /api/trips/{tripId}/stops/{stopId}/depart</summary>
        public Task<ApiResponse<object>> DepartStopAsync(int tripId, int stopId)
            => _api.PostAsync<object>($"api/trips/{tripId}/stops/{stopId}/depart");
    }
}