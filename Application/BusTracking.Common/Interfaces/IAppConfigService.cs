namespace BusTracking.Common.Interfaces
{
    public interface IAppConfigService
    {
        Task<ApiResponse<List<AppConfigDto>>> GetAllAsync(string? platform, string? search, bool? isActive);
        Task<ApiResponse<AppConfigDto>> GetByIdAsync(int configId);
        Task<ApiResponse<bool>> CreateAsync(CreateAppConfigDto dto, int createdBy);
        Task<ApiResponse<bool>> UpdateAsync(int configId, UpdateAppConfigDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int configId);
        Task<ApiResponse<bool>> ToggleActiveAsync(int configId);

        /// <summary>Returns only active key-value pairs for a given platform (used by MAUI apps)</summary>
        Task<ApiResponse<List<AppConfigValueDto>>> GetConfigForPlatformAsync(ConfigPlatform platform);
    }
}