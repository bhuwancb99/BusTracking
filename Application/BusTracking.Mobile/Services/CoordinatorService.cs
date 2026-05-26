using BusTracking.Mobile.Interfaces;
using BusTracking.Mobile.Models.Common;
using BusTracking.Mobile.Models.Coordinator;

namespace BusTracking.Mobile.Services
{
    public class CoordinatorService : ICoordinatorService
    {
        private readonly IApiService _api;
        public CoordinatorService(IApiService api) => _api = api;

        public async Task<List<CoordinatorItem>> GetAllAsync(string? search = null, int page = 1)
        {
            var url = $"{Constants.Admin.Coordinators}?page={page}" + (search != null ? $"&search={Uri.EscapeDataString(search)}" : "");
            var r = await _api.GetAsync<PagedResult<CoordinatorItem>>(url);
            return r.Data?.Items ?? [];
        }

        public async Task<CoordinatorItem?> GetByIdAsync(int id)
        {
            var r = await _api.GetAsync<CoordinatorItem>(string.Format(Constants.Admin.CoordinatorById, id));
            return r.Data;
        }

        public Task<ApiResponse<object>> CreateAsync(CreateCoordinatorRequest req) => _api.PostAsync<object>(Constants.Admin.Coordinators, req);
        public Task<ApiResponse<object>> UpdateAsync(int id, UpdateCoordinatorRequest req) => _api.PutAsync<object>(string.Format(Constants.Admin.CoordinatorById, id), req);
        public Task<ApiResponse<object>> DeleteAsync(int id) => _api.DeleteAsync<object>(string.Format(Constants.Admin.CoordinatorById, id));
        public Task<ApiResponse<object>> ToggleAsync(int id) => _api.PostAsync<object>(string.Format(Constants.Admin.CoordinatorToggle, id));
        public Task<ApiResponse<object>> ResetPasswordAsync(int id) => _api.PostAsync<object>(string.Format(Constants.Admin.CoordinatorReset, id));

        public async Task<List<PermissionItem>> GetAllPermissionsAsync()
        {
            var r = await _api.GetAsync<List<PermissionItem>>(Constants.Admin.AllPermissions);
            return r.Data ?? [];
        }

        public async Task<List<int>> GetAssignedPermissionsAsync(int id)
        {
            var r = await _api.GetAsync<dynamic>(string.Format(Constants.Admin.CoordinatorPerms, id));
            return [];
        }
    }
}
