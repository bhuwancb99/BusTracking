namespace BusTracking.Mobile.Services
{
    public class ParentService : IParentService
    {
        private readonly IApiService _api;
        private readonly IAuthService _auth;

        public ParentService(IApiService api, IAuthService auth) { _api = api; _auth = auth; }

        private string BaseUrl => _auth.CurrentRole == Constants.Roles.SuperAdmin
            ? Constants.Admin.Parents : Constants.Coordinator.Parents;

        public async Task<List<ParentItem>> GetAllAsync(string? search = null, int page = 1)
        {
            var url = $"{BaseUrl}?page={page}" + (search != null ? $"&search={Uri.EscapeDataString(search)}" : "");
            var r = await _api.GetAsync<PagedResult<ParentItem>>(url);
            return r.Data?.Items ?? [];
        }

        public async Task<ParentItem?> GetByIdAsync(int id)
        {
            var r = await _api.GetAsync<ParentItem>(string.Format(Constants.Admin.ParentById, id));
            return r.Data;
        }

        public Task<ApiResponse<object>> CreateAsync(CreateParentRequest req) => _api.PostAsync<object>(BaseUrl, req);
        public Task<ApiResponse<object>> UpdateAsync(int id, UpdateParentRequest req) => _api.PutAsync<object>(string.Format(Constants.Admin.ParentById, id), req);
        public Task<ApiResponse<object>> DeleteAsync(int id) => _api.DeleteAsync<object>(string.Format(Constants.Admin.ParentById, id));
        public Task<ApiResponse<object>> ToggleAsync(int id) => _api.PostAsync<object>(string.Format(Constants.Admin.ParentToggle, id));
        public Task<ApiResponse<object>> ResetPasswordAsync(int id) => _api.PostAsync<object>(string.Format(Constants.Admin.ParentReset, id));

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
