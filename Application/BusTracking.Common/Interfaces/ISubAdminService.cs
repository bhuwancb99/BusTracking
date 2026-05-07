using BusTracking.Common.DTOs.Common;
using BusTracking.Common.DTOs.SubAdmin;

namespace BusTracking.Common.Interfaces
{
    public interface ISubAdminService
    {
        Task<ApiResponse<PagedResult<SubAdminListDto>>> GetAllAsync(int page, int pageSize, string? search);
        Task<ApiResponse<SubAdminListDto>> GetByIdAsync(int userId);
        Task<ApiResponse<bool>> CreateAsync(CreateSubAdminDto dto, int createdBy);
        Task<ApiResponse<bool>> UpdateAsync(int userId, UpdateSubAdminDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int userId);
        Task<ApiResponse<bool>> ToggleActiveAsync(int userId);
    }
}
