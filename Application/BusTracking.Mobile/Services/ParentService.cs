namespace BusTracking.Mobile.Services
{
    public class ParentService : IParentService
    {
        private readonly IApiService _api;
        private readonly IAuthService _auth;

        public ParentService(IApiService api, IAuthService auth) { _api = api; _auth = auth; }

        private string BaseUrl => _auth.CurrentRole == Constants.Roles.SuperAdmin
            ? Constants.Admin.Parents : Constants.Coordinator.Parents;

        private bool IsCoordinator => _auth.CurrentRole == Constants.Roles.BusCoordinator;

        public async Task<PagedResult<ParentItem>> GetAllAsync(string? search = null, int page = 1, string? status = "Active")
        {
            var url = $"{BaseUrl}?page={page}";
            if (!string.IsNullOrWhiteSpace(search))
                url += $"&search={Uri.EscapeDataString(search)}";
            if (!string.IsNullOrWhiteSpace(status))
                url += $"&status={Uri.EscapeDataString(status)}";

            var r = await _api.GetAsync<PagedResult<ParentItem>>(url);
            return r.Data ?? new PagedResult<ParentItem>();
        }

        // Unpaginated lookup for dropdowns/pickers and the Coordinator "find by id"
        // workaround — walks all pages so every active parent is reachable.
        public async Task<List<ParentItem>> GetAllForFormAsync(string? search = null)
        {
            var all = new List<ParentItem>();
            int page = 1;
            PagedResult<ParentItem> data;
            do
            {
                data = await GetAllAsync(search, page, "Active");
                all.AddRange(data.Items);
                page++;
            } while (page <= data.TotalPages);

            return all;
        }

        public async Task<ParentItem?> GetByIdAsync(int id)
        {
            var url = IsCoordinator
                ? string.Format(Constants.Coordinator.ParentById, id)
                : string.Format(Constants.Admin.ParentById, id);
            var r = await _api.GetAsync<ParentItem>(url);
            return r.Data;
        }

        public async Task<List<StudentSearchItem>> SearchStudentsAsync(string query)
        {
            var url = IsCoordinator
                ? $"{Constants.Coordinator.StudentSearch}?query={Uri.EscapeDataString(query)}"
                : $"{Constants.Admin.StudentSearch}?query={Uri.EscapeDataString(query)}";
            var r = await _api.GetAsync<List<StudentSearchItem>>(url);
            return r.Data ?? [];
        }

        public Task<ApiResponse<object>> CreateAsync(CreateParentRequest req)
            => _api.PostAsync<object>(BaseUrl, req);

        public Task<ApiResponse<object>> UpdateAsync(int id, UpdateParentRequest req)
        {
            var url = IsCoordinator
                ? string.Format(Constants.Coordinator.ParentById, id)
                : string.Format(Constants.Admin.ParentById, id);
            return _api.PutAsync<object>(url, req);
        }

        public Task<ApiResponse<object>> DeleteAsync(int id)
        {
            var url = IsCoordinator
                ? string.Format(Constants.Coordinator.ParentById, id)
                : string.Format(Constants.Admin.ParentById, id);
            return _api.DeleteAsync<object>(url);
        }

        public Task<ApiResponse<object>> ToggleAsync(int id)
        {
            var url = IsCoordinator
                ? string.Format(Constants.Coordinator.ParentToggle, id)
                : string.Format(Constants.Admin.ParentToggle, id);
            return _api.PostAsync<object>(url);
        }

        public Task<ApiResponse<ResetPasswordResult>> ResetPasswordAsync(int id)
            => _api.PostAsync<ResetPasswordResult>(string.Format(Constants.Admin.ParentReset, id));

        public async Task<object?> GetDashboardAsync()
        {
            var r = await _api.GetAsync<object>(Constants.Parent.Dashboard);
            return r.Data;
        }

        public async Task<TrackingData?> TrackChildBusAsync(int studentId)
        {
            var r = await _api.GetAsync<TrackingData>(string.Format(Constants.Parent.TrackBus, studentId));
            return r.Data;
        }
    }
}
