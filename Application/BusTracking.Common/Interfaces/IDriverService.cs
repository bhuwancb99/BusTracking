using BusTracking.Common.DTOs.Assign;
using BusTracking.Common.DTOs.Common;
using BusTracking.Common.DTOs.Driver;
using BusTracking.Common.DTOs.User;

namespace BusTracking.Common.Interfaces
{
    public interface IDriverService
    {
        Task<ApiResponse<PagedResult<DriverListDto>>> GetAllAsync(int page, int pageSize, string? search, string? status);
        Task<ApiResponse<DriverListDto>> GetByIdAsync(int userId);
        Task<ApiResponse<CreatedUserResultDto>> CreateAsync(CreateDriverDto dto, int createdBy);
        Task<ApiResponse<bool>> UpdateAsync(int userId, UpdateDriverDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int userId);
        Task<ApiResponse<bool>> ToggleActiveAsync(int userId);
        Task<ApiResponse<bool>> AssignBusAsync(AssignBusToDriverDto dto);
        Task<ApiResponse<List<DriverDropdownDto>>> GetDropdownAsync(string? search);
    }
}
