namespace BusTracking.Common.Interfaces
{
    public interface IStudentExtService
    {
        Task<ApiResponse<StudentDetailViewDto>> GetDetailAsync(int studentId);

        Task<ApiResponse<CreatedUserResultDto>> CreateExtAsync(CreateStudentExtDto dto, int createdBy);

        Task<ApiResponse<bool>> UpdateExtAsync(int studentId, UpdateStudentExtDto dto);
    }
}
