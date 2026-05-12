using BusTracking.Common.DTOs.Student;

namespace BusTracking.Common.Interfaces
{
    public interface IStudentSearchService
    {
        Task<List<StudentSearchDto>> SearchAsync(string query, int maxResults = 10);
    }
}
