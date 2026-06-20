namespace BusTracking.Mobile.Services
{
    public class CoordSubAdminService : ICoordSubAdminService
    {
        private readonly IApiService _api;
        public CoordSubAdminService(IApiService api) => _api = api;

        public async Task<PagedResult<CoordinatorItem>> GetAllAsync(string? search = null, string? status = null, int page = 1)
        {
            var url = $"{Constants.Coordinator.SubAdmins}?page={page}";
            if (!string.IsNullOrWhiteSpace(search)) url += $"&search={Uri.EscapeDataString(search)}";
            if (!string.IsNullOrWhiteSpace(status)) url += $"&status={status}";
            var r = await _api.GetAsync<PagedResult<CoordinatorItem>>(url);
            return r.Data ?? new PagedResult<CoordinatorItem>();
        }

        public async Task<CoordinatorItem?> GetByIdAsync(int id)
        {
            var r = await _api.GetAsync<CoordinatorItem>(string.Format(Constants.Coordinator.SubAdminById, id));
            return r.Data;
        }

        public Task<ApiResponse<object>> CreateAsync(CreateCoordinatorRequest req)
            => _api.PostAsync<object>(Constants.Coordinator.SubAdmins, req);

        public Task<ApiResponse<object>> UpdateAsync(int id, UpdateCoordinatorRequest req)
            => _api.PutAsync<object>(string.Format(Constants.Coordinator.SubAdminById, id), req);

        public Task<ApiResponse<object>> DeleteAsync(int id)
            => _api.DeleteAsync<object>(string.Format(Constants.Coordinator.SubAdminById, id));

        public Task<ApiResponse<object>> ToggleAsync(int id)
            => _api.PostAsync<object>(string.Format(Constants.Coordinator.SubAdminToggle, id));

        public Task<ApiResponse<object>> ResetPasswordAsync(int id)
            => _api.PostAsync<object>(string.Format(Constants.Coordinator.SubAdminReset, id));

        public async Task<List<PermissionItem>> GetAllPermissionsAsync()
        {
            var r = await _api.GetAsync<List<PermissionItem>>(Constants.Coordinator.CoordAllPermissions);
            return r.Data ?? [];
        }

        public async Task<List<int>> GetAssignedPermissionsAsync(int id)
        {
            var r = await _api.GetAsync<CoordinatorPermissionsResponse>(
                string.Format(Constants.Coordinator.SubAdminPerms, id));
            return r.Data?.AssignedPermissionIds ?? [];
        }
    }
}
