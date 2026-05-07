using BusTracking.Common.DTOs.Common;
using BusTracking.Common.DTOs.Parent;

namespace BusTracking.Common.Interfaces
{
    public interface IParentService
    {
        Task<ApiResponse<PagedResult<ParentListDto>>> GetAllAsync(int page, int pageSize, string? search);
        Task<ApiResponse<ParentListDto>> GetByIdAsync(int userId);
        Task<ApiResponse<bool>> CreateAsync(CreateParentDto dto, int createdBy);
        Task<ApiResponse<bool>> UpdateAsync(int userId, UpdateParentDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int userId);
    }
}
