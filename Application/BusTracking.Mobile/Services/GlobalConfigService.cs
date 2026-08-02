namespace BusTracking.Mobile.Services
{
    /// <summary>
    /// Pre-login global configuration service.
    /// Reads from GlobalConfigurations table via /api/global-config/mobile (anonymous).
    /// Separate from AppConfigService which reads school-scoped AppConfigurations after login.
    /// </summary>
    public class GlobalConfigService : IGlobalConfigService
    {
        private readonly IApiService _api;
        private readonly ICacheService _cache;

        public GlobalConfigService(IApiService api, ICacheService cache)
        {
            _api = api;
            _cache = cache;
        }

        public async Task<Dictionary<string, string>> GetGlobalConfigAsync(bool forceRefresh = false)
        {
            const string key = Constants.Cache.GlobalConfig;
            var ttl = TimeSpan.FromHours(Constants.Cache.GlobalConfigTtlH);

            if (!forceRefresh && _cache.Has(key))
                return _cache.Get<Dictionary<string, string>>(key) ?? [];

            var r = await _api.GetAsync<List<AppConfigValue>>(Constants.GlobalConfig.Mobile);
            if (!r.Success || r.Data is null) return [];

            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in r.Data)
            {
                if (!string.IsNullOrEmpty(item.Key))
                {
                    dict[item.Key] = item.Value ?? "";
                }
            }
            _cache.Set(key, dict, ttl);
            return dict;
        }

        public async Task<string?> GetValueAsync(string key)
        {
            var config = await GetGlobalConfigAsync();
            return config.TryGetValue(key, out var v) ? v : null;
        }

        public async Task<bool> IsMaintenanceModeAsync()
        {
            var v = await GetValueAsync("IsMaintencePage");
            return string.Equals(v, "true", StringComparison.OrdinalIgnoreCase) || v == "1";
        }

        public async Task<bool> IsMandatoryUpdateAsync()
        {
            var v = await GetValueAsync("MandatoryUpdateApp");
            return string.Equals(v, "true", StringComparison.OrdinalIgnoreCase) || v == "1";
        }
    }
}
