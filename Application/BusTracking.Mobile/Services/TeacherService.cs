namespace BusTracking.Mobile.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly IApiService _api;

        public TeacherService(IApiService api)
        {
            _api = api;
        }

        public async Task<PagedResult<TeacherItem>> GetTeachersAsync(int page = 1, string? search = null, string? status = null, int? schoolId = null, bool isCoordinator = false)
        {
            var baseUrl = isCoordinator ? Constants.Coordinator.Teachers : Constants.Admin.Teachers;
            var url = $"{baseUrl}?page={page}&pageSize=10";
            if (!string.IsNullOrWhiteSpace(search)) url += $"&search={Uri.EscapeDataString(search)}";
            if (schoolId.HasValue && schoolId.Value > 0) url += $"&schoolId={schoolId.Value}";

            var res = await _api.GetAsync<PagedResult<TeacherItem>>(url);
            return res.Data ?? new PagedResult<TeacherItem>();
        }

        public async Task<TeacherItem?> GetTeacherByIdAsync(int teacherId, bool isCoordinator = false)
        {
            var baseUrl = isCoordinator ? Constants.Coordinator.TeacherById : Constants.Admin.TeacherById;
            var url = string.Format(baseUrl, teacherId);

            var res = await _api.GetAsync<TeacherItem>(url);
            return res.Data;
        }

        public async Task<ApiResponse<object>> CreateTeacherAsync(CreateTeacherRequest req, bool isCoordinator = false)
        {
            var url = isCoordinator ? Constants.Coordinator.Teachers : Constants.Admin.Teachers;
            var res = await _api.PostAsync<object>(url, req);
            return res ?? ApiResponse<object>.Fail("Failed to create teacher account.");
        }

        public async Task<ApiResponse<object>> UpdateTeacherAsync(int teacherId, UpdateTeacherRequest req, bool isCoordinator = false)
        {
            var baseUrl = isCoordinator ? Constants.Coordinator.TeacherById : Constants.Admin.TeacherById;
            var url = string.Format(baseUrl, teacherId);

            var res = await _api.PutAsync<object>(url, req);
            return res ?? ApiResponse<object>.Fail("Failed to update teacher account.");
        }

        public async Task<ApiResponse<object>> ToggleTeacherStatusAsync(int teacherId, bool isCoordinator = false)
        {
            var baseUrl = isCoordinator ? Constants.Coordinator.TeacherToggle : Constants.Admin.TeacherToggle;
            var url = string.Format(baseUrl, teacherId);

            var res = await _api.PostAsync<object>(url, new { });
            return res ?? ApiResponse<object>.Fail("Failed to toggle teacher status.");
        }

        public async Task<ApiResponse<object>> DeleteTeacherAsync(int teacherId, bool isCoordinator = false)
        {
            var baseUrl = isCoordinator ? Constants.Coordinator.TeacherDelete : Constants.Admin.TeacherDelete;
            var url = string.Format(baseUrl, teacherId);

            var res = await _api.DeleteAsync<object>(url);
            return res ?? ApiResponse<object>.Fail("Failed to delete teacher account.");
        }

        public async Task<ApiResponse<ResetPasswordResult>> ResetPasswordAsync(int teacherId, bool isCoordinator = false)
        {
            var baseUrl = isCoordinator ? Constants.Coordinator.TeacherReset : Constants.Admin.TeacherReset;
            var url = string.Format(baseUrl, teacherId);

            var res = await _api.PostAsync<ResetPasswordResult>(url, new { });
            return res ?? ApiResponse<ResetPasswordResult>.Fail("Failed to reset teacher password.");
        }

        public async Task<TeacherItem?> GetMyProfileAsync()
        {
            var res = await _api.GetAsync<TeacherItem>(Constants.Teacher.Profile);
            return res.Data;
        }
    }
}
