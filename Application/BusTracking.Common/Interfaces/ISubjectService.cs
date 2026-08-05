namespace BusTracking.Common.Interfaces
{
    public interface ISubjectService
    {
        Task<ApiResponse<PagedResult<SubjectDto>>> GetAllAsync(string? search, bool? isActive, int page = 1, int pageSize = 10);
        Task<ApiResponse<List<SubjectDto>>> GetActiveSubjectsAsync();
        Task<ApiResponse<SubjectDto>> GetByIdAsync(int subjectId);
        Task<ApiResponse<SubjectDto>> CreateAsync(CreateSubjectDto dto);
        Task<ApiResponse<bool>> UpdateAsync(int subjectId, UpdateSubjectDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int subjectId);
        Task<ApiResponse<bool>> ToggleActiveAsync(int subjectId);
    }
}
