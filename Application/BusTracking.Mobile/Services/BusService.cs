namespace BusTracking.Mobile.Services
{
    public class BusService : IBusService
    {
        private readonly IApiService _api;
        private readonly ICacheService _cache;
        private readonly IAuthService _auth;

        public BusService(IApiService api, ICacheService cache, IAuthService auth)
        { _api = api; _cache = cache; _auth = auth; }

        private string BaseEndpoint => _auth.CurrentRole == Constants.Roles.SuperAdmin
            ? Constants.Admin.Buses : Constants.Coordinator.Buses;

        public async Task<PagedResult<BusItem>> GetAllAsync(string? search = null, int page = 1, string? status = "Active")
        {
            var url = $"{BaseEndpoint}?page={page}";
            if (!string.IsNullOrWhiteSpace(search))
                url += $"&search={Uri.EscapeDataString(search)}";
            if (!string.IsNullOrWhiteSpace(status))
                url += $"&status={Uri.EscapeDataString(status)}";

            var r = await _api.GetAsync<PagedResult<BusItem>>(url);
            return r.Data ?? new PagedResult<BusItem>();
        }

        // Unpaginated lookup for dropdowns/pickers that need full BusItem fields
        // (Trip/Student forms) — walks all pages so every active bus is selectable.
        public async Task<List<BusItem>> GetAllForFormAsync(string? search = null)
        {
            var all = new List<BusItem>();
            int page = 1;
            PagedResult<BusItem> data;
            do
            {
                data = await GetAllAsync(search, page, "Active");
                all.AddRange(data.Items);
                page++;
            } while (page <= data.TotalPages);

            return all;
        }

        public async Task<BusItem?> GetByIdAsync(int id)
        {
            var url = _auth.CurrentRole == Constants.Roles.SuperAdmin
                ? string.Format(Constants.Admin.BusById, id)
                : string.Format(Constants.Coordinator.BusById, id);
            var r = await _api.GetAsync<BusItem>(url);
            return r.Data;
        }

        public async Task<ApiResponse<object>> CreateAsync(CreateBusRequest req)
            => await _api.PostAsync<object>(BaseEndpoint, req);

        public async Task<ApiResponse<object>> UpdateAsync(int id, UpdateBusRequest req)
            => await _api.PutAsync<object>(string.Format(Constants.Admin.BusById, id), req);

        public async Task<ApiResponse<object>> DeleteAsync(int id)
            => await _api.DeleteAsync<object>(string.Format(Constants.Admin.BusById, id));

        public async Task<ApiResponse<object>> ToggleAsync(int id)
            => await _api.PostAsync<object>(string.Format(Constants.Admin.BusToggle, id));

        // Dropdown always returns only ACTIVE buses (for assignment in forms)
        public async Task<List<DropdownItem>> GetDropdownAsync(string? search = null)
        {
            var url = Constants.Admin.BusDropdown + "?isActive=true"
                + (search != null ? $"&search={Uri.EscapeDataString(search)}" : "");
            var r = await _api.GetAsync<List<DropdownItem>>(url);
            return r.Data ?? [];
        }
    }
}