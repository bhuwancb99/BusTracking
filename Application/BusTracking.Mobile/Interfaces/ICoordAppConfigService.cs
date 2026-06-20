namespace BusTracking.Mobile.Interfaces
{
    public interface ICoordAppConfigService
    {
        Task<PagedResult<AppConfigItem>> GetAllAsync(string? platform = null, string? search = null, int page = 1);
        Task<AppConfigItem?> GetByIdAsync(int id);
        Task<ApiResponse<object>> CreateAsync(CreateAppConfigRequest req);
        Task<ApiResponse<object>> UpdateAsync(int id, UpdateAppConfigRequest req);
        Task<ApiResponse<object>> DeleteAsync(int id);
        Task<ApiResponse<object>> ToggleAsync(int id);
    }
}
