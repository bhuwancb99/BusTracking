using BusTracking.Mobile.Interfaces;
using BusTracking.Mobile.Models.AppConfig;
using BusTracking.Mobile.Models.Common;

namespace BusTracking.Mobile.Services
{
    public class AdminConfigService : IAdminConfigService
    {
        private readonly IApiService _api;
        public AdminConfigService(IApiService api) => _api = api;

        public async Task<List<AppConfigItem>> GetAllAsync(string? platform = null)
        {
            var url = Constants.Admin.Config + (platform != null ? $"?platform={platform}" : "");
            var r = await _api.GetAsync<PagedResult<AppConfigItem>>(url);
            return r.Data?.Items ?? [];
        }

        public async Task<AppConfigItem?> GetByIdAsync(int id)
        {
            var r = await _api.GetAsync<AppConfigItem>(string.Format(Constants.Admin.ConfigById, id));
            return r.Data;
        }

        public Task<ApiResponse<object>> CreateAsync(CreateAppConfigRequest req) => _api.PostAsync<object>(Constants.Admin.Config, req);
        public Task<ApiResponse<object>> UpdateAsync(int id, UpdateAppConfigRequest req) => _api.PutAsync<object>(string.Format(Constants.Admin.ConfigById, id), req);
        public Task<ApiResponse<object>> DeleteAsync(int id) => _api.DeleteAsync<object>(string.Format(Constants.Admin.ConfigById, id));
        public Task<ApiResponse<object>> ToggleAsync(int id) => _api.PostAsync<object>(string.Format(Constants.Admin.ConfigToggle, id));
    }
}
