namespace BusTracking.Common.Interfaces
{
    public interface IParentService
    {
        Task<ApiResponse<PagedResult<ParentListDto>>> GetAllAsync(int page, int pageSize, string? search, string? status);
        Task<ApiResponse<ParentListDto>> GetByIdAsync(int userId);
        Task<ApiResponse<CreatedUserResultDto>> CreateAsync(CreateParentDto dto, int createdBy);
        Task<ApiResponse<bool>> UpdateAsync(int userId, UpdateParentDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int userId);
        Task<ApiResponse<bool>> ToggleActiveAsync(int userId);
    }
}
