using BusTracking.Common.DTOs.Availability;
using BusTracking.Common.DTOs.Common;
using BusTracking.Common.DTOs.Student;

namespace BusTracking.Common.Interfaces
{
    public interface IStudentService
    {
        Task<ApiResponse<PagedResult<StudentListDto>>> GetAllAsync(int page, int pageSize, string? search);
        Task<ApiResponse<StudentListDto>> GetByIdAsync(int studentId);
        Task<ApiResponse<bool>> CreateAsync(CreateStudentDto dto, int createdBy);
        Task<ApiResponse<bool>> UpdateAsync(int studentId, UpdateStudentDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int studentId);
        Task<ApiResponse<bool>> SetAvailabilityAsync(CreateAvailabilityDto dto, int markedBy);
        Task<ApiResponse<List<AvailabilityDto>>> GetAvailabilitiesAsync(int studentId);
    }
}
