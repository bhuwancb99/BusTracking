namespace BusTracking.Mobile.Services
{
    public class DriverService : IDriverService
    {
        private readonly IApiService _api;
        private readonly IAuthService _auth;

        public DriverService(IApiService api, IAuthService auth) { _api = api; _auth = auth; }

        private string BaseUrl => _auth.CurrentRole == Constants.Roles.SuperAdmin
            ? Constants.Admin.Drivers : Constants.Coordinator.Drivers;

        private bool IsCoordinator => _auth.CurrentRole == Constants.Roles.BusCoordinator;

        public async Task<PagedResult<DriverItem>> GetAllAsync(string? search = null, int page = 1, string? status = "Active")
        {
            var url = $"{BaseUrl}?page={page}";
            if (!string.IsNullOrWhiteSpace(search))
                url += $"&search={Uri.EscapeDataString(search)}";
            if (!string.IsNullOrWhiteSpace(status))
                url += $"&status={Uri.EscapeDataString(status)}";

            var r = await _api.GetAsync<PagedResult<DriverItem>>(url);
            return r.Data ?? new PagedResult<DriverItem>();
        }

        // Unpaginated lookup for dropdowns/pickers that need full DriverItem fields
        // (Trip/Bus forms) — walks all pages so every active driver is selectable.
        public async Task<List<DriverItem>> GetAllForFormAsync(string? search = null)
        {
            var all = new List<DriverItem>();
            int page = 1;
            PagedResult<DriverItem> data;
            do
            {
                data = await GetAllAsync(search, page, "Active");
                all.AddRange(data.Items);
                page++;
            } while (page <= data.TotalPages);

            return all;
        }

        public async Task<DriverItem?> GetByIdAsync(int id)
        {
            // Coordinator has no /{id} endpoint — fetch from list and find by id
            if (IsCoordinator)
            {
                var list = await GetAllForFormAsync();
                return list.FirstOrDefault(d => d.UserId == id);
            }
            var r = await _api.GetAsync<DriverItem>(string.Format(Constants.Admin.DriverById, id));
            return r.Data;
        }

        public Task<ApiResponse<object>> CreateAsync(CreateDriverRequest req)
            => _api.PostAsync<object>(BaseUrl, req);

        public Task<ApiResponse<object>> UpdateAsync(int id, UpdateDriverRequest req)
            => _api.PutAsync<object>(string.Format(Constants.Admin.DriverById, id), req);

        public Task<ApiResponse<object>> DeleteAsync(int id)
            => _api.DeleteAsync<object>(string.Format(Constants.Admin.DriverById, id));

        public Task<ApiResponse<object>> ToggleAsync(int id)
            => _api.PostAsync<object>(string.Format(Constants.Admin.DriverToggle, id));

        public Task<ApiResponse<ResetPasswordResult>> ResetPasswordAsync(int id)
            => _api.PostAsync<ResetPasswordResult>(string.Format(Constants.Admin.DriverReset, id));

        public async Task<List<DropdownItem>> GetDropdownAsync(string? search = null)
        {
            var url = Constants.Admin.DriverDropdown + "?isActive=true"
                + (search != null ? $"&search={Uri.EscapeDataString(search)}" : "");
            var r = await _api.GetAsync<List<DropdownItem>>(url);
            return r.Data ?? [];
        }

        public async Task<List<DriverNotificationItem>> GetAllNotificationAsync()
        {
            var r = await _api.GetAsync<List<DriverNotificationItem>>(
                Constants.Driver.Notifications);
            return r.Data ?? [];
        }

        public Task<ApiResponse<object>> MarkReadAsync(int notificationId) =>
            _api.PostAsync<object>(
                string.Format(Constants.Driver.NotifMarkRead, notificationId));

        public Task<ApiResponse<object>> MarkAllReadAsync() =>
            _api.PostAsync<object>(Constants.Driver.NotifMarkAllRead);
    }
}
