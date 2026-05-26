using BusTracking.Mobile.Interfaces;
using BusTracking.Mobile.Models.Bus;
using BusTracking.Mobile.Models.Common;

namespace BusTracking.Mobile.Services
{
    public class BusService : IBusService
    {
        private readonly IApiService _api;
        private readonly ICacheService _cache;
        private readonly IAuthService _auth;
        private string AdminEndpoint(string t) => _auth.CurrentRole == Constants.Roles.SuperAdmin ? t.Replace("coordinator", "admin") : t;

        public BusService(IApiService api, ICacheService cache, IAuthService auth)
        { _api = api; _cache = cache; _auth = auth; }

        private string BaseEndpoint => _auth.CurrentRole == Constants.Roles.SuperAdmin
            ? Constants.Admin.Buses : Constants.Coordinator.Buses;

        public async Task<List<BusItem>> GetAllAsync(string? search = null, int page = 1)
        {
            var url = $"{BaseEndpoint}?page={page}" + (search != null ? $"&search={Uri.EscapeDataString(search)}" : "");
            var r = await _api.GetAsync<PagedResult<BusItem>>(url);
            return r.Data?.Items ?? [];
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

        public async Task<List<DropdownItem>> GetDropdownAsync(string? search = null)
        {
            var url = Constants.Admin.BusDropdown + (search != null ? $"?search={Uri.EscapeDataString(search)}" : "");
            var r = await _api.GetAsync<List<DropdownItem>>(url);
            return r.Data ?? [];
        }
    }
}
