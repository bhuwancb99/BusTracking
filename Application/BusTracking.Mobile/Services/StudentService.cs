namespace BusTracking.Mobile.Services
{
    public class StudentService : IStudentService
    {
        private readonly IApiService _api;
        private readonly IAuthService _auth;

        public StudentService(IApiService api, IAuthService auth) { _api = api; _auth = auth; }

        private string BaseUrl => _auth.CurrentRole == Constants.Roles.SuperAdmin
            ? Constants.Admin.Students : Constants.Coordinator.Students;

        private bool IsCoordinator => _auth.CurrentRole == Constants.Roles.BusCoordinator;

        public async Task<List<StudentItem>> GetAllAsync(string? search = null, int page = 1, bool? isActive = true)
        {
            if (IsCoordinator)
            {
                var url = BaseUrl + (search != null ? $"?search={Uri.EscapeDataString(search)}" : "");
                var r = await _api.GetAsync<List<StudentItem>>(url);
                return r.Data ?? [];
            }
            else
            {
                var url = $"{BaseUrl}?page={page}";
                if (!string.IsNullOrWhiteSpace(search))
                    url += $"&search={Uri.EscapeDataString(search)}";
                if (isActive.HasValue)
                    url += $"&isActive={isActive.Value.ToString().ToLower()}";
                var r = await _api.GetAsync<PagedResult<StudentItem>>(url);
                return r.Data?.Items ?? [];
            }
        }

        public async Task<StudentItem?> GetByIdAsync(int id)
        {
            var url = _auth.CurrentRole == Constants.Roles.SuperAdmin
                ? string.Format(Constants.Admin.StudentById, id)
                : string.Format(Constants.Coordinator.StudentById, id);
            var r = await _api.GetAsync<StudentItem>(url);
            return r.Data;
        }

        public Task<ApiResponse<object>> CreateAsync(CreateStudentRequest req)
            => _api.PostAsync<object>(BaseUrl, req);

        public Task<ApiResponse<object>> UpdateAsync(int id, UpdateStudentRequest req)
            => _api.PutAsync<object>(string.Format(Constants.Admin.StudentById, id), req);

        public Task<ApiResponse<object>> DeleteAsync(int id)
            => _api.DeleteAsync<object>(string.Format(Constants.Admin.StudentById, id));

        public Task<ApiResponse<object>> ToggleAsync(int id)
            => _api.PostAsync<object>(string.Format(Constants.Admin.StudentToggle, id));

        public Task<ApiResponse<object>> ResetPasswordAsync(int id)
            => _api.PostAsync<object>(string.Format(Constants.Admin.StudentReset, id));

        public async Task<List<StudentItem>> SearchAsync(string query)
        {
            var url = $"{BaseUrl}?search={Uri.EscapeDataString(query)}&isActive=true";
            var r = await _api.GetAsync<PagedResult<StudentItem>>(url);
            return r.Data?.Items ?? [];
        }

        public async Task<TrackingData?> GetTrackingAsync()
        {
            var r = await _api.GetAsync<TrackingData>(Constants.Student.Tracking);
            return r.Data;
        }

        public Task<ApiResponse<bool>> SetAvailabilityAsync(object req)
            => _api.PostAsync<bool>(Constants.Student.Availability, req);
    }
}