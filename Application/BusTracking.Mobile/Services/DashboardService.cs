using BusTracking.Mobile.Interfaces;
using BusTracking.Mobile.Models.Dashboard;

namespace BusTracking.Mobile.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IApiService _api;
        private readonly ICacheService _cache;
        private readonly IAuthService _auth;

        public DashboardService(IApiService api, ICacheService cache, IAuthService auth)
        {
            _api = api; _cache = cache; _auth = auth;
        }

        public async Task<DashboardSummary?> GetAdminSummaryAsync(bool forceRefresh = false)
        {
            const string key = Constants.Cache.Dashboard;
            var ttl = TimeSpan.FromMinutes(Constants.Cache.DashboardTtlM);

            if (!forceRefresh && _cache.Has(key))
                return _cache.Get<DashboardSummary>(key);

            var endpoint = _auth.CurrentRole == Constants.Roles.SuperAdmin
                ? Constants.Admin.Dashboard
                : Constants.Coordinator.Dashboard;

            var r = await _api.GetAsync<DashboardSummary>(endpoint);
            if (!r.Success || r.Data is null) return null;
            _cache.Set(key, r.Data, ttl);
            return r.Data;
        }
    }
}
