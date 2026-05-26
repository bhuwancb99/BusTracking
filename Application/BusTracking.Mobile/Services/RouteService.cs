using BusTracking.Mobile.Interfaces;
using BusTracking.Mobile.Models.Common;
using BusTracking.Mobile.Models.Route;

namespace BusTracking.Mobile.Services
{
    public class RouteService : IRouteService
    {
        private readonly IApiService _api;
        private readonly ICacheService _cache;
        private readonly IAuthService _auth;

        public RouteService(IApiService api, ICacheService cache, IAuthService auth)
        { _api = api; _cache = cache; _auth = auth; }

        private string BaseUrl => _auth.CurrentRole == Constants.Roles.SuperAdmin
            ? Constants.Admin.Routes : Constants.Coordinator.Routes;

        public async Task<List<RouteItem>> GetAllAsync()
        {
            const string key = "routes_list";
            if (_cache.Has(key)) return _cache.Get<List<RouteItem>>(key) ?? [];
            var r = await _api.GetAsync<PagedResult<RouteItem>>(BaseUrl);
            var list = r.Data?.Items ?? [];
            _cache.Set(key, list, TimeSpan.FromMinutes(Constants.Cache.ListTtlM));
            return list;
        }

        public async Task<List<StopItem>> GetStopsAsync(int routeId)
        {
            var key = $"route_stops_{routeId}";
            if (_cache.Has(key)) return _cache.Get<List<StopItem>>(key) ?? [];
            var url = _auth.CurrentRole == Constants.Roles.SuperAdmin
                ? string.Format(Constants.Admin.RouteStops, routeId)
                : string.Format(Constants.Coordinator.RouteStops, routeId);
            var r = await _api.GetAsync<List<StopItem>>(url);
            var list = r.Data ?? [];
            _cache.Set(key, list, TimeSpan.FromMinutes(Constants.Cache.ListTtlM));
            return list;
        }

        public Task<ApiResponse<object>> CreateAsync(CreateRouteRequest req) => _api.PostAsync<object>(Constants.Admin.Routes, req);
        public Task<ApiResponse<object>> UpdateAsync(int id, UpdateRouteRequest req) => _api.PutAsync<object>(string.Format(Constants.Admin.RouteById, id), req);
        public Task<ApiResponse<object>> DeleteAsync(int id) => _api.DeleteAsync<object>(string.Format(Constants.Admin.RouteById, id));
    }
}
