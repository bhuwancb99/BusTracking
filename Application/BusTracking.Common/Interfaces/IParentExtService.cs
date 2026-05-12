using BusTracking.Common.DTOs.Common;
using BusTracking.Common.DTOs.Parent;
using BusTracking.Common.DTOs.User;

namespace BusTracking.Common.Interfaces
{
    public interface IParentExtService
    {
        Task<ApiResponse<ParentDetailViewDto>> GetDetailAsync(int userId);

        Task<ApiResponse<CreatedUserResultDto>> CreateExtAsync(CreateParentExtDto dto, int createdBy);

        Task<ApiResponse<bool>> UpdateExtAsync(int userId, UpdateParentExtDto dto);
    }

}
