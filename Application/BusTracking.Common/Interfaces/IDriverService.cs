using BusTracking.Common.DTOs.Common;
using BusTracking.Common.DTOs.Driver;

namespace BusTracking.Common.Interfaces
{
    public interface IDriverService
    {
        Task<ApiResponse<PagedResult<DriverListDto>>> GetAllAsync(int page, int pageSize, string? search);
        Task<ApiResponse<DriverListDto>> GetByIdAsync(int userId);
        Task<ApiResponse<bool>> CreateAsync(CreateDriverDto dto, int createdBy);
        Task<ApiResponse<bool>> UpdateAsync(int userId, UpdateDriverDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int userId);
        Task<ApiResponse<bool>> AssignBusAsync(int driverUserId, int busId);
    }
}
