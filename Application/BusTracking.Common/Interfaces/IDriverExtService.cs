using BusTracking.Common.DTOs.Common;
using BusTracking.Common.DTOs.Driver;
using BusTracking.Common.DTOs.User;

namespace BusTracking.Common.Interfaces
{
    public interface IDriverExtService
    {
        Task<ApiResponse<DriverDetailViewDto>> GetDetailAsync(int userId);

        Task<ApiResponse<CreatedUserResultDto>> CreateExtAsync(CreateDriverExtDto dto, int createdBy);

        Task<ApiResponse<bool>> UpdateExtAsync(int userId, UpdateDriverExtDto dto);

        Task<ApiResponse<bool>> ToggleActiveAsync(int userId);
    }
}
