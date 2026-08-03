namespace BusTracking.Common.Interfaces
{
    public interface ITeacherService
    {
        Task<PagedResult<TeacherDto>> GetTeachersAsync(int? schoolId, string? search, int page, int pageSize);
        Task<ApiResponse<TeacherDto>> GetTeacherByIdAsync(int teacherId);
        Task<ApiResponse<TeacherDto>> GetTeacherByUserIdAsync(int userId);
        Task<ApiResponse<TeacherDto>> CreateTeacherAsync(CreateTeacherDto dto, string? profileImageUrl = null);
        Task<ApiResponse<TeacherDto>> UpdateTeacherAsync(UpdateTeacherDto dto, string? profileImageUrl = null);
        Task<ApiResponse<bool>> ToggleTeacherStatusAsync(int teacherId);
        Task<ApiResponse<bool>> DeleteTeacherAsync(int teacherId);
        Task<ApiResponse<CreatedUserResultDto>> ResetPasswordAsync(int teacherId);
        Task<ApiResponse<bool>> CheckUsernameAvailabilityAsync(string userName, int? excludeUserId = null);
    }
}
