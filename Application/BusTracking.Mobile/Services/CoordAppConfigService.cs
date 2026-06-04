namespace BusTracking.Mobile.Services
{
    public class CoordAppConfigService : ICoordAppConfigService
    {
        private readonly IApiService _api;
        public CoordAppConfigService(IApiService api) => _api = api;

        public async Task<List<AppConfigItem>> GetAllAsync(string? platform = null)
        {
            var url = Constants.Coordinator.Config;
            var q = new List<string>();
            if (!string.IsNullOrWhiteSpace(platform)) q.Add($"platform={platform}");
            if (q.Count > 0) url += "?" + string.Join("&", q);
            var r = await _api.GetAsync<List<AppConfigItem>>(url);
            return r.Data ?? [];
        }

        public async Task<AppConfigItem?> GetByIdAsync(int id)
        {
            var r = await _api.GetAsync<AppConfigItem>(string.Format(Constants.Coordinator.ConfigById, id));
            return r.Data;
        }

        public Task<ApiResponse<object>> CreateAsync(CreateAppConfigRequest req)
            => _api.PostAsync<object>(Constants.Coordinator.Config, req);

        public Task<ApiResponse<object>> UpdateAsync(int id, UpdateAppConfigRequest req)
            => _api.PutAsync<object>(string.Format(Constants.Coordinator.ConfigById, id), req);

        public Task<ApiResponse<object>> DeleteAsync(int id)
            => _api.DeleteAsync<object>(string.Format(Constants.Coordinator.ConfigById, id));

        public Task<ApiResponse<object>> ToggleAsync(int id)
            => _api.PostAsync<object>(string.Format(Constants.Coordinator.ConfigToggle, id));
    }
}
