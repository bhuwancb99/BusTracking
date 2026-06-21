namespace BusTracking.Common.Interfaces
{
    public interface IStudentService
    {
        Task<ApiResponse<PagedResult<StudentListDto>>> GetAllAsync(int page, string? search, string? status);
        Task<int> GetListPageSizeAsync();
        Task<ApiResponse<StudentListDto>> GetByIdAsync(int studentId);
        Task<ApiResponse<CreatedUserResultDto>> CreateAsync(CreateStudentDto dto, int createdBy);
        Task<ApiResponse<bool>> UpdateAsync(int studentId, UpdateStudentDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int studentId);
        Task<ApiResponse<bool>> ToggleActiveAsync(int studentId);
        Task<ApiResponse<bool>> AssignBusAsync(AssignBusToStudentDto dto);
        Task<ApiResponse<bool>> SetAvailabilityAsync(CreateAvailabilityDto dto, int markedBy);
        Task<ApiResponse<List<AvailabilityDto>>> GetAvailabilitiesAsync(int studentId);
        Task<ApiResponse<List<StudentSearchDto>>> SearchAsync(string? query);
        Task<ApiResponse<CreatedUserResultDto>> ResetPasswordAsync(int studentId);
    }
}
