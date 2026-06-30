namespace BusTracking.Mobile.Services
{
    public class CoordinatorService : ICoordinatorService
    {
        private readonly IApiService _api;
        public CoordinatorService(IApiService api) => _api = api;

        public async Task<PagedResult<CoordinatorItem>> GetAllAsync(string? search = null, string? status = null, int page = 1)
        {
            var url = Constants.Admin.Coordinators + $"?page={page}";
            if (!string.IsNullOrWhiteSpace(search))
                url += $"&search={Uri.EscapeDataString(search)}";
            if (!string.IsNullOrWhiteSpace(status))
                url += $"&status={status}";
            var r = await _api.GetAsync<PagedResult<CoordinatorItem>>(url);
            return r.Data ?? new PagedResult<CoordinatorItem>();
        }

        public async Task<CoordinatorItem?> GetByIdAsync(int id)
        {
            var r = await _api.GetAsync<CoordinatorItem>(string.Format(Constants.Admin.CoordinatorById, id));
            return r.Data;
        }

        public Task<ApiResponse<object>> CreateAsync(CreateCoordinatorRequest req)
            => _api.PostAsync<object>(Constants.Admin.Coordinators, req);

        public Task<ApiResponse<object>> UpdateAsync(int id, UpdateCoordinatorRequest req)
            => _api.PutAsync<object>(string.Format(Constants.Admin.CoordinatorById, id), req);

        public Task<ApiResponse<object>> DeleteAsync(int id)
            => _api.DeleteAsync<object>(string.Format(Constants.Admin.CoordinatorById, id));

        public Task<ApiResponse<object>> ToggleAsync(int id)
            => _api.PostAsync<object>(string.Format(Constants.Admin.CoordinatorToggle, id));

        public Task<ApiResponse<ResetPasswordResult>> ResetPasswordAsync(int id)
            => _api.PostAsync<ResetPasswordResult>(string.Format(Constants.Admin.CoordinatorReset, id));

        public async Task<List<PermissionItem>> GetAllPermissionsAsync()
        {
            var r = await _api.GetAsync<List<PermissionItem>>(Constants.Admin.AllPermissions);
            return r.Data ?? [];
        }

        public async Task<List<int>> GetAssignedPermissionsAsync(int id)
        {
            // GET /api/admin/coordinators/{id}/permissions returns a combined object:
            //   { "assignedPermissionIds": [...], "allPermissions": [...] }
            // NOT a plain List<int> — deserialize the wrapper and extract the ids.
            var r = await _api.GetAsync<CoordinatorPermissionsResponse>(
                string.Format(Constants.Admin.CoordinatorPerms, id));
            return r.Data?.AssignedPermissionIds ?? [];
        }
    }
}