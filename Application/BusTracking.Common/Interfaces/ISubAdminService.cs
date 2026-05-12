using BusTracking.Common.DTOs.Common;
using BusTracking.Common.DTOs.SubAdmin;
using BusTracking.Common.DTOs.User;

namespace BusTracking.Common.Interfaces
{
    public interface ISubAdminService
    {
        Task<ApiResponse<PagedResult<SubAdminListDto>>> GetAllAsync(int page, int pageSize, string? search, string? status);
        Task<ApiResponse<SubAdminListDto>> GetByIdAsync(int userId);
        Task<ApiResponse<CreatedUserResultDto>> CreateAsync(CreateSubAdminDto dto, int createdBy);
        Task<ApiResponse<bool>> UpdateAsync(int userId, UpdateSubAdminDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int userId);
        Task<ApiResponse<bool>> ToggleActiveAsync(int userId);
    }
}
