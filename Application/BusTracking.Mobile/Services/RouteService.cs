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

        private bool IsSuperAdmin => _auth.CurrentRole == Constants.Roles.SuperAdmin;

        private string BaseUrl => IsSuperAdmin ? Constants.Admin.Routes : Constants.Coordinator.Routes;

        private string RouteByIdUrl(int id) => IsSuperAdmin
            ? string.Format(Constants.Admin.RouteById, id)
            : string.Format(Constants.Coordinator.RouteById, id);

        private string RouteStopsUrl(int id) => IsSuperAdmin
            ? string.Format(Constants.Admin.RouteStops, id)
            : string.Format(Constants.Coordinator.RouteStops, id);

        private string RouteStopDeleteUrl(int stopId) => IsSuperAdmin
            ? string.Format(Constants.Admin.RouteStopDelete, stopId)
            : string.Format(Constants.Coordinator.RouteStopDelete, stopId);

        public async Task<PagedResult<RouteItem>> GetAllAsync(string? search = null, int page = 1, string? status = "Active")
        {
            var url = $"{BaseUrl}?page={page}";
            if (!string.IsNullOrWhiteSpace(search))
                url += $"&search={Uri.EscapeDataString(search)}";
            if (!string.IsNullOrWhiteSpace(status))
                url += $"&status={Uri.EscapeDataString(status)}";

            var r = await _api.GetAsync<PagedResult<RouteItem>>(url);
            return r.Data ?? new PagedResult<RouteItem>();
        }

        // Unpaginated lookup for dropdowns/pickers (Add Bus, Add Trip, etc.)
        // — walks all pages so every active route is available to select.
        public async Task<List<RouteItem>> GetDropdownAsync(string? search = null)
        {
            var all = new List<RouteItem>();
            int page = 1;
            PagedResult<RouteItem> data;
            do
            {
                data = await GetAllAsync(search, page, "Active");
                all.AddRange(data.Items);
                page++;
            } while (page <= data.TotalPages);

            return all;
        }

        public async Task<RouteItem?> GetByIdAsync(int id)
        {
            var r = await _api.GetAsync<RouteItem>(RouteByIdUrl(id));
            return r.Data;
        }

        public async Task<List<StopItem>> GetStopsAsync(int routeId)
        {
            var key = $"route_stops_{routeId}";
            if (_cache.Has(key)) return _cache.Get<List<StopItem>>(key) ?? [];
            var r = await _api.GetAsync<List<StopItem>>(RouteStopsUrl(routeId));
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
            var result = await _api.PutAsync<object>(RouteByIdUrl(id), req);
            if (result.Success) { _cache.Remove(ListCacheKey); _cache.Remove($"route_stops_{id}"); }
            return result;
        }

        public async Task<ApiResponse<object>> DeleteAsync(int id)
        {
            var result = await _api.DeleteAsync<object>(RouteByIdUrl(id));
            if (result.Success) { _cache.Remove(ListCacheKey); _cache.Remove($"route_stops_{id}"); }
            return result;
        }

        public async Task<ApiResponse<object>> AddStopAsync(CreateStopRequest req)
        {
            var url = IsSuperAdmin
                ? string.Format(Constants.Admin.RouteStops, req.RouteId)
                : string.Format(Constants.Coordinator.RouteStops, req.RouteId);
            var result = await _api.PostAsync<object>(url, req);
            if (result.Success) { _cache.Remove(ListCacheKey); _cache.Remove($"route_stops_{req.RouteId}"); }
            return result;
        }

        public async Task<ApiResponse<object>> DeleteStopAsync(int stopId, int routeId)
        {
            var result = await _api.DeleteAsync<object>(RouteStopDeleteUrl(stopId));
            if (result.Success) { _cache.Remove(ListCacheKey); _cache.Remove($"route_stops_{routeId}"); }
            return result;
        }
    }
}