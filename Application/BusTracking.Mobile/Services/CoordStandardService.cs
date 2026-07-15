namespace BusTracking.Mobile.Services
{
    public class CoordStandardService : ICoordStandardService
    {
        private readonly IApiService _api;
        public CoordStandardService(IApiService api) => _api = api;

        public async Task<PagedResult<StandardItem>> GetAllAsync(string? search = null, int page = 1)
        {
            var url = $"{Constants.Coordinator.Standards}?page={page}";
            if (!string.IsNullOrWhiteSpace(search))
                url += $"&search={Uri.EscapeDataString(search)}";

            var r = await _api.GetAsync<PagedResult<StandardItem>>(url);
            return r.Data ?? new PagedResult<StandardItem>();
        }

        public async Task<StandardItem?> GetByIdAsync(int id)
        {
            var r = await _api.GetAsync<StandardItem>(string.Format(Constants.Coordinator.StandardById, id));
            return r.Data;
        }

        public Task<ApiResponse<object>> CreateAsync(CreateStandardRequest req)
            => _api.PostAsync<object>(Constants.Coordinator.Standards, req);

        public Task<ApiResponse<object>> UpdateAsync(int id, UpdateStandardRequest req)
            => _api.PutAsync<object>(string.Format(Constants.Coordinator.StandardById, id), req);

        public Task<ApiResponse<object>> DeleteAsync(int id)
            => _api.DeleteAsync<object>(string.Format(Constants.Coordinator.StandardById, id));

        public Task<ApiResponse<object>> ToggleAsync(int id)
            => _api.PostAsync<object>(string.Format(Constants.Coordinator.StandardToggle, id));
    }
}
