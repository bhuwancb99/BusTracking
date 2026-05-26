using BusTracking.Mobile.Interfaces;
using BusTracking.Mobile.Models.Common;
using BusTracking.Mobile.Models.Trip;

namespace BusTracking.Mobile.Services
{
    public class TripService : ITripService
    {
        private readonly IApiService _api;
        private readonly IAuthService _auth;

        public TripService(IApiService api, IAuthService auth) { _api = api; _auth = auth; }

        private bool IsAdmin => _auth.CurrentRole == Constants.Roles.SuperAdmin;

        public async Task<List<TripItem>> GetAllAsync(string? status = null, string? date = null)
        {
            var url = IsAdmin ? Constants.Admin.Trips : Constants.Coordinator.Trips;
            var q = new List<string>();
            if (status != null) q.Add($"status={status}");
            if (date != null) q.Add($"date={date}");
            if (q.Count > 0) url += "?" + string.Join("&", q);
            var r = await _api.GetAsync<PagedResult<TripItem>>(url);
            return r.Data?.Items ?? [];
        }

        public Task<ApiResponse<object>> CreateAsync(CreateTripRequest req) => _api.PostAsync<object>(
            IsAdmin ? Constants.Admin.Trips : Constants.Coordinator.Trips, req);

        public Task<ApiResponse<object>> StartAsync(int id) => _api.PostAsync<object>(
            string.Format(IsAdmin ? Constants.Admin.TripStart : Constants.Coordinator.TripStart, id));

        public Task<ApiResponse<object>> EndAsync(int id) => _api.PostAsync<object>(
            string.Format(IsAdmin ? Constants.Admin.TripEnd : Constants.Coordinator.TripEnd, id));

        public Task<ApiResponse<object>> CancelAsync(int id) => _api.PostAsync<object>(
            string.Format(IsAdmin ? Constants.Admin.TripCancel : Constants.Coordinator.TripCancel, id));

        public async Task<BusLocation?> GetLocationAsync(int tripId)
        {
            var url = string.Format(IsAdmin ? Constants.Admin.TripLocation : Constants.Coordinator.TripLocation, tripId);
            var r = await _api.GetAsync<BusLocation>(url);
            return r.Data;
        }
    }
}
