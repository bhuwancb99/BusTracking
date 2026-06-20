namespace BusTracking.Common.Interfaces
{
    public interface ISubAdminService
    {
        Task<ApiResponse<PagedResult<SubAdminListDto>>> GetAllAsync(int page, string? search, string? status);
        Task<ApiResponse<SubAdminListDto>> GetByIdAsync(int userId);
        Task<ApiResponse<CreatedUserResultDto>> CreateAsync(CreateSubAdminDto dto, int createdBy);
        Task<ApiResponse<bool>> UpdateAsync(int userId, UpdateSubAdminDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int userId);
        Task<ApiResponse<bool>> ToggleActiveAsync(int userId);
        Task<ApiResponse<CreatedUserResultDto>> ResetPasswordAsync(int userId);
        Task<List<int>> GetPermissionIdsAsync(int userId);
        Task<List<(int Id, string ModuleName, string Key, string Description)>> GetAllPermissionsAsync();
        Task<int> GetListPageSizeAsync();
    }
}
