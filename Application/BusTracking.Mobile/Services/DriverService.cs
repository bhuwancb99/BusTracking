namespace BusTracking.Mobile.Services
{
    public class DriverService : IDriverService
    {
        private readonly IApiService _api;
        private readonly IAuthService _auth;

        public DriverService(IApiService api, IAuthService auth) { _api = api; _auth = auth; }

        private string BaseUrl => _auth.CurrentRole == Constants.Roles.SuperAdmin
            ? Constants.Admin.Drivers : Constants.Coordinator.Drivers;

        public async Task<List<DriverItem>> GetAllAsync(string? search = null, int page = 1)
        {
            var url = $"{BaseUrl}?page={page}" + (search != null ? $"&search={Uri.EscapeDataString(search)}" : "");
            var r = await _api.GetAsync<PagedResult<DriverItem>>(url);
            return r.Data?.Items ?? [];
        }

        public async Task<DriverItem?> GetByIdAsync(int id)
        {
            var r = await _api.GetAsync<DriverItem>(string.Format(Constants.Admin.DriverById, id));
            return r.Data;
        }

        public Task<ApiResponse<object>> CreateAsync(CreateDriverRequest req) => _api.PostAsync<object>(BaseUrl, req);
        public Task<ApiResponse<object>> UpdateAsync(int id, UpdateDriverRequest req) => _api.PutAsync<object>(string.Format(Constants.Admin.DriverById, id), req);
        public Task<ApiResponse<object>> DeleteAsync(int id) => _api.DeleteAsync<object>(string.Format(Constants.Admin.DriverById, id));
        public Task<ApiResponse<object>> ToggleAsync(int id) => _api.PostAsync<object>(string.Format(Constants.Admin.DriverToggle, id));
        public Task<ApiResponse<object>> ResetPasswordAsync(int id) => _api.PostAsync<object>(string.Format(Constants.Admin.DriverReset, id));

        public async Task<List<DropdownItem>> GetDropdownAsync(string? search = null)
        {
            var url = Constants.Admin.DriverDropdown + (search != null ? $"?search={Uri.EscapeDataString(search)}" : "");
            var r = await _api.GetAsync<List<DropdownItem>>(url);
            return r.Data ?? [];
        }
    }
}
