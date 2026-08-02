namespace BusTracking.Common.Interfaces
{
    public interface IGlobalConfigService
    {
        Task<PagedResult<GlobalConfigDto>> GetAllAsync(string? search = null, bool? isActive = null, int page = 1, int pageSize = 20);
        Task<ApiResponse<GlobalConfigDto>> GetByIdAsync(int id);
        Task<ApiResponse<GlobalConfigDto>> GetByKeyAsync(string key);
        Task<ApiResponse<GlobalConfigDto>> CreateAsync(CreateGlobalConfigDto dto);
        Task<ApiResponse<GlobalConfigDto>> UpdateAsync(int id, UpdateGlobalConfigDto dto);
        Task<ApiResponse<bool>> ToggleActiveAsync(int id);
    }
}
