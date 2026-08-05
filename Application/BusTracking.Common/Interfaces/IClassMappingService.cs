namespace BusTracking.Common.Interfaces
{
    public interface IClassMappingService
    {
        Task<ApiResponse<List<ClassSubjectTeacherDto>>> GetClassMappingsAsync(int academicYearId, int standardId, int? sectionId);
        Task<ApiResponse<ClassSubjectTeacherDto>> GetByIdAsync(int id);
        Task<ApiResponse<ClassSubjectTeacherDto>> AssignSubjectTeacherAsync(AssignClassSubjectTeacherDto dto);
        Task<ApiResponse<ClassSubjectTeacherDto>> UpdateSubjectTeacherAsync(int id, AssignClassSubjectTeacherDto dto);
        Task<ApiResponse<bool>> UnassignSubjectTeacherAsync(int classSubjectTeacherId);
    }
}
