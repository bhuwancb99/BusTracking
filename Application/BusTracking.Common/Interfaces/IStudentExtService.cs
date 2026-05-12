using BusTracking.Common.DTOs.Common;
using BusTracking.Common.DTOs.Student;
using BusTracking.Common.DTOs.User;

namespace BusTracking.Common.Interfaces
{
    public interface IStudentExtService
    {
        Task<ApiResponse<StudentDetailViewDto>> GetDetailAsync(int studentId);

        Task<ApiResponse<CreatedUserResultDto>> CreateExtAsync(CreateStudentExtDto dto, int createdBy);

        Task<ApiResponse<bool>> UpdateExtAsync(int studentId, UpdateStudentExtDto dto);
    }
}
