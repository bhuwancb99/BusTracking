namespace BusTracking.Mobile.Services
{
    public class SubjectService : ISubjectService
    {
        private readonly IApiService _api;
        public SubjectService(IApiService api) => _api = api;

        public async Task<PagedResult<SubjectItem>> GetAllAsync(string? search = null, int page = 1, bool isCoordinator = false)
        {
            var baseEndpoint = isCoordinator ? Constants.Coordinator.Subjects : Constants.Admin.Subjects;
            var url = $"{baseEndpoint}?page={page}";
            if (!string.IsNullOrWhiteSpace(search)) url += $"&search={Uri.EscapeDataString(search)}";

            var r = await _api.GetAsync<PagedResult<SubjectItem>>(url);
            return r.Data ?? new PagedResult<SubjectItem>();
        }

        public async Task<SubjectItem?> GetByIdAsync(int id, bool isCoordinator = false)
        {
            var endpoint = isCoordinator ? Constants.Coordinator.SubjectById : Constants.Admin.SubjectById;
            var r = await _api.GetAsync<SubjectItem>(string.Format(endpoint, id));
            return r.Data;
        }

        public Task<ApiResponse<object>> CreateAsync(CreateSubjectRequest req, bool isCoordinator = false)
        {
            var endpoint = isCoordinator ? Constants.Coordinator.Subjects : Constants.Admin.Subjects;
            return _api.PostAsync<object>(endpoint, req);
        }

        public Task<ApiResponse<object>> UpdateAsync(int id, UpdateSubjectRequest req, bool isCoordinator = false)
        {
            var endpoint = isCoordinator ? Constants.Coordinator.SubjectById : Constants.Admin.SubjectById;
            return _api.PutAsync<object>(string.Format(endpoint, id), req);
        }

        public Task<ApiResponse<object>> DeleteAsync(int id, bool isCoordinator = false)
        {
            var endpoint = isCoordinator ? Constants.Coordinator.SubjectById : Constants.Admin.SubjectById;
            return _api.DeleteAsync<object>(string.Format(endpoint, id));
        }

        public Task<ApiResponse<object>> ToggleAsync(int id, bool isCoordinator = false)
        {
            var endpoint = isCoordinator ? Constants.Coordinator.SubjectToggle : Constants.Admin.SubjectToggle;
            return _api.PostAsync<object>(string.Format(endpoint, id));
        }
    }
}
