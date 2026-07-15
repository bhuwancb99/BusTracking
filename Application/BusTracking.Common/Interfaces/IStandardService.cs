namespace BusTracking.Common.Interfaces
{
    public interface IStandardService
    {
        Task<ApiResponse<PagedResult<StandardDto>>> GetAllAsync(string? search, bool? isActive, int page = 1);
        Task<ApiResponse<StandardDto>> GetByIdAsync(int standardId);
        Task<ApiResponse<bool>> CreateAsync(CreateStandardDto dto);
        Task<ApiResponse<bool>> UpdateAsync(int standardId, UpdateStandardDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int standardId);
        Task<ApiResponse<bool>> ToggleActiveAsync(int standardId);
        Task<ApiResponse<List<StandardDto>>> GetActiveStandardsAsync();
    }
}
