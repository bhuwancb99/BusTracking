namespace BusTracking.Mobile.Services
{
    public class BusTypeService : IBusTypeService
    {
        private readonly IApiService _api;
        public BusTypeService(IApiService api) => _api = api;

        public async Task<PagedResult<BusTypeItem>> GetAllAsync(string? search = null, int page = 1)
        {
            var url = $"{Constants.BusType.All}?page={page}";
            if (!string.IsNullOrWhiteSpace(search))
                url += $"&search={Uri.EscapeDataString(search)}";

            var r = await _api.GetAsync<PagedResult<BusTypeItem>>(url);
            return r.Data ?? new PagedResult<BusTypeItem>();
        }

        public Task<ApiResponse<object>> CreateAsync(string name) =>
            _api.PostAsync<object>(Constants.BusType.All, new { Name = name });

        public Task<ApiResponse<object>> UpdateAsync(int id, string name) =>
            _api.PutAsync<object>(string.Format(Constants.BusType.ById, id), new { Name = name });

        public Task<ApiResponse<object>> DeleteAsync(int id) =>
            _api.DeleteAsync<object>(string.Format(Constants.BusType.ById, id));
    }
}
