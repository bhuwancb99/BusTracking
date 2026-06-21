namespace BusTracking.Mobile.Services
{
    public class TripService : ITripService
    {
        private readonly IApiService _api;
        private readonly IAuthService _auth;

        public TripService(IApiService api, IAuthService auth) { _api = api; _auth = auth; }

        private bool IsAdmin => _auth.CurrentRole == Constants.Roles.SuperAdmin;

        public async Task<PagedResult<TripItem>> GetAllAsync(string? status = null, string? date = null, int page = 1)
        {
            var url = IsAdmin ? Constants.Admin.Trips : Constants.Coordinator.Trips;
            var q = new List<string> { $"page={page}" };
            if (!string.IsNullOrWhiteSpace(status)) q.Add($"status={Uri.EscapeDataString(status)}");
            if (!string.IsNullOrWhiteSpace(date)) q.Add($"date={Uri.EscapeDataString(date)}");
            url += "?" + string.Join("&", q);

            var r = await _api.GetAsync<PagedResult<TripItem>>(url);
            return r.Data ?? new PagedResult<TripItem>();
        }

        public async Task<TripItem?> GetByIdAsync(int id)
        {
            var url = IsAdmin
                ? string.Format(Constants.Admin.TripById, id)
                : string.Format(Constants.Coordinator.TripById, id);
            var r = await _api.GetAsync<TripItem>(url);
            return r.Data;
        }

        public Task<ApiResponse<object>> CreateAsync(CreateTripRequest req) => _api.PostAsync<object>(
            IsAdmin ? Constants.Admin.Trips : Constants.Coordinator.Trips, req);

        public Task<ApiResponse<object>> StartAsync(int id) => _api.PostAsync<object>(
            string.Format(IsAdmin ? Constants.Admin.TripStart : Constants.Coordinator.TripStart, id));

        public Task<ApiResponse<object>> EndAsync(int id) => _api.PostAsync<object>(
            string.Format(IsAdmin ? Constants.Admin.TripEnd : Constants.Coordinator.TripEnd, id));

        public Task<ApiResponse<object>> CancelAsync(int id) => _api.PostAsync<object>(
            string.Format(IsAdmin ? Constants.Admin.TripCancel : Constants.Coordinator.TripCancel, id));

        public Task<ApiResponse<object>> DeleteAsync(int id) => _api.DeleteAsync<object>(
            string.Format(IsAdmin ? Constants.Admin.TripById : Constants.Coordinator.TripById, id));

        public async Task<BusLocation?> GetLocationAsync(int tripId)
        {
            var url = string.Format(IsAdmin ? Constants.Admin.TripLocation : Constants.Coordinator.TripLocation, tripId);
            var r = await _api.GetAsync<BusLocation>(url);
            return r.Data;
        }
    }
}
