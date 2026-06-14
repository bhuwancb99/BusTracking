namespace BusTracking.Mobile.Services
{
    public class BusTypeService : IBusTypeService
    {
        private readonly IApiService _api;
        public BusTypeService(IApiService api) => _api = api;

        public async Task<List<BusTypeItem>> GetAllAsync(string? search = null)
        {
            var url = Constants.BusType.All;
            if (!string.IsNullOrWhiteSpace(search))
                url += $"?search={Uri.EscapeDataString(search)}";

            var r = await _api.GetAsync<List<BusTypeItem>>(url);
            return r.Data ?? [];
        }

        public Task<ApiResponse<object>> CreateAsync(string name) =>
            _api.PostAsync<object>(Constants.BusType.All, new { Name = name });

        public Task<ApiResponse<object>> UpdateAsync(int id, string name) =>
            _api.PutAsync<object>(string.Format(Constants.BusType.ById, id), new { Name = name });

        public Task<ApiResponse<object>> DeleteAsync(int id) =>
            _api.DeleteAsync<object>(string.Format(Constants.BusType.ById, id));
    }
}
