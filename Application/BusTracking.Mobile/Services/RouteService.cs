namespace BusTracking.Mobile.Services
{
    public class RouteService : IRouteService
    {
        private readonly IApiService _api;
        private readonly ICacheService _cache;
        private readonly IAuthService _auth;

        private const string ListCacheKey = "routes_list";

        public RouteService(IApiService api, ICacheService cache, IAuthService auth)
        { _api = api; _cache = cache; _auth = auth; }

        private string BaseUrl => _auth.CurrentRole == Constants.Roles.SuperAdmin
            ? Constants.Admin.Routes : Constants.Coordinator.Routes;

        public async Task<List<RouteItem>> GetAllAsync()
        {
            if (_auth.CurrentRole == Constants.Roles.BusCoordinator)
            {
                var r = await _api.GetAsync<List<RouteItem>>(BaseUrl);
                return r.Data ?? [];
            }
            else
            {
                var r = await _api.GetAsync<PagedResult<RouteItem>>(BaseUrl);
                return r.Data?.Items ?? [];
            }
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

        public async Task<ApiResponse<object>> CreateAsync(CreateRouteRequest req)
        {
            var result = await _api.PostAsync<object>(BaseUrl, req);
            if (result.Success) _cache.Remove(ListCacheKey);
            return result;
        }

        public async Task<ApiResponse<object>> UpdateAsync(int id, UpdateRouteRequest req)
        {
            var result = await _api.PutAsync<object>(string.Format(Constants.Admin.RouteById, id), req);
            if (result.Success) { _cache.Remove(ListCacheKey); _cache.Remove($"route_stops_{id}"); }
            return result;
        }

        public async Task<ApiResponse<object>> DeleteAsync(int id)
        {
            var result = await _api.DeleteAsync<object>(string.Format(Constants.Admin.RouteById, id));
            if (result.Success) { _cache.Remove(ListCacheKey); _cache.Remove($"route_stops_{id}"); }
            return result;
        }
    }
}