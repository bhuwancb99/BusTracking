using BusTracking.Mobile.Models.AppConfig;
using BusTracking.Mobile.Models.Common;

namespace BusTracking.Mobile.Interfaces
{
    public interface IAdminConfigService
    {
        Task<List<AppConfigItem>> GetAllAsync(string? platform = null);
        Task<AppConfigItem?> GetByIdAsync(int id);
        Task<ApiResponse<object>> CreateAsync(CreateAppConfigRequest req);
        Task<ApiResponse<object>> UpdateAsync(int id, UpdateAppConfigRequest req);
        Task<ApiResponse<object>> DeleteAsync(int id);
        Task<ApiResponse<object>> ToggleAsync(int id);
    }
}
