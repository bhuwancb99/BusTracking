using BusTracking.Common.DTOs.Bus;
using BusTracking.Common.DTOs.Common;

namespace BusTracking.Common.Interfaces
{
    public interface IBusService
    {
        Task<ApiResponse<PagedResult<BusListDto>>> GetAllAsync(int page, int pageSize, string? search);
        Task<ApiResponse<BusDetailDto>> GetByIdAsync(int busId);
        Task<ApiResponse<bool>> CreateAsync(CreateBusDto dto, int createdBy);
        Task<ApiResponse<bool>> UpdateAsync(int busId, UpdateBusDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int busId);
        Task<ApiResponse<bool>> AssignStudentAsync(int busId, int studentId);
        Task<ApiResponse<bool>> RemoveStudentAsync(int busId, int studentId);
    }
}
