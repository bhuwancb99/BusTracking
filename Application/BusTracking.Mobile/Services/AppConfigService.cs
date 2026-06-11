namespace BusTracking.Mobile.Services
{
    public class AppConfigService : IAppConfigService
    {
        private readonly IApiService _api;
        private readonly ICacheService _cache;

        public AppConfigService(IApiService api, ICacheService cache)
        {
            _api = api; _cache = cache;
        }

        public async Task<Dictionary<string, string>> GetMobileConfigAsync(bool forceRefresh = false)
        {
            const string key = Constants.Cache.AppConfig;
            var ttl = TimeSpan.FromHours(Constants.Cache.AppConfigTtlH);

            if (!forceRefresh && _cache.Has(key))
                return _cache.Get<Dictionary<string, string>>(key) ?? [];

            var r = await _api.GetAsync<List<AppConfigValue>>(Constants.AppConfig.Mobile);
            if (!r.Success || r.Data is null) return [];

            var dict = r.Data.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
            _cache.Set(key, dict, ttl);
            return dict;
        }

        public async Task<string?> GetValueAsync(string key)
        {
            var config = await GetMobileConfigAsync();
            return config.TryGetValue(key, out var v) ? v : null;
        }

        public async Task<bool> IsMaintenanceModeAsync()
        {
            var v = await GetValueAsync("IsMaintencePage");
            return v == "1";
        }

        public async Task<bool> IsMandatoryUpdateAsync()
        {
            var v = await GetValueAsync("MandatoryUpdateApp");
            return v == "1";
        }

        /// <summary>
        /// Returns true when IsMobileUpdateImage = "1".
        /// When true: app uploads images via API and shows Upload/Remove buttons.
        /// When false: app only displays images using WebsiteImageUrl as base.
        /// </summary>
        public async Task<bool> IsMobileImageUpdateEnabledAsync()
        {
            var v = await GetValueAsync("IsMobileUpdateImage");
            return v == "1";
        }

        /// <summary>
        /// Returns the base URL stored in WebsiteImageUrl config key.
        /// Used to construct full image URLs when IsMobileUpdateImage = 0.
        /// E.g. "https://website.com" + "/media/images/driver/u_5.jpg"
        /// </summary>
        public async Task<string?> GetWebsiteImageUrlAsync()
        {
            return await GetValueAsync("WebsiteImageUrl");
        }
    }
}
