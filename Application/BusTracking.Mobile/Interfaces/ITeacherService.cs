namespace BusTracking.Mobile.Interfaces
{
    public interface ITeacherService
    {
        Task<PagedResult<TeacherItem>> GetTeachersAsync(int page = 1, string? search = null, string? status = null, int? schoolId = null, bool isCoordinator = false);
        Task<TeacherItem?> GetTeacherByIdAsync(int teacherId, bool isCoordinator = false);
        Task<ApiResponse<object>> CreateTeacherAsync(CreateTeacherRequest req, bool isCoordinator = false);
        Task<ApiResponse<object>> UpdateTeacherAsync(int teacherId, UpdateTeacherRequest req, bool isCoordinator = false);
        Task<ApiResponse<object>> ToggleTeacherStatusAsync(int teacherId, bool isCoordinator = false);
        Task<ApiResponse<object>> DeleteTeacherAsync(int teacherId, bool isCoordinator = false);
        Task<ApiResponse<ResetPasswordResult>> ResetPasswordAsync(int teacherId, bool isCoordinator = false);
        Task<TeacherItem?> GetMyProfileAsync();
    }
}
