using BusTracking.Mobile.Models.Common;
using BusTracking.Mobile.Models.Coordinator;

namespace BusTracking.Mobile.Services
{
    public interface ICoordinatorService
    {
        Task<List<CoordinatorItem>> GetAllAsync(string? search = null, int page = 1);
        Task<CoordinatorItem?> GetByIdAsync(int id);
        Task<ApiResponse<object>> CreateAsync(CreateCoordinatorRequest req);
        Task<ApiResponse<object>> UpdateAsync(int id, UpdateCoordinatorRequest req);
        Task<ApiResponse<object>> DeleteAsync(int id);
        Task<ApiResponse<object>> ToggleAsync(int id);
        Task<ApiResponse<object>> ResetPasswordAsync(int id);
        Task<List<PermissionItem>> GetAllPermissionsAsync();
        Task<List<int>> GetAssignedPermissionsAsync(int id);
    }
}
