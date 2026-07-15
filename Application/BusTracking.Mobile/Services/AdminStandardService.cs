namespace BusTracking.Mobile.Services
{
    public class AdminStandardService : IAdminStandardService
    {
        private readonly IApiService _api;
        public AdminStandardService(IApiService api) => _api = api;

        public async Task<PagedResult<StandardItem>> GetAllAsync(string? search = null, int page = 1)
        {
            var url = $"{Constants.Admin.Standards}?page={page}";
            if (!string.IsNullOrWhiteSpace(search))
                url += $"&search={Uri.EscapeDataString(search)}";

            var r = await _api.GetAsync<PagedResult<StandardItem>>(url);
            return r.Data ?? new PagedResult<StandardItem>();
        }

        public async Task<StandardItem?> GetByIdAsync(int id)
        {
            var r = await _api.GetAsync<StandardItem>(string.Format(Constants.Admin.StandardById, id));
            return r.Data;
        }

        public Task<ApiResponse<object>> CreateAsync(CreateStandardRequest req)
            => _api.PostAsync<object>(Constants.Admin.Standards, req);

        public Task<ApiResponse<object>> UpdateAsync(int id, UpdateStandardRequest req)
            => _api.PutAsync<object>(string.Format(Constants.Admin.StandardById, id), req);

        public Task<ApiResponse<object>> DeleteAsync(int id)
            => _api.DeleteAsync<object>(string.Format(Constants.Admin.StandardById, id));

        public Task<ApiResponse<object>> ToggleAsync(int id)
            => _api.PostAsync<object>(string.Format(Constants.Admin.StandardToggle, id));
    }
}
