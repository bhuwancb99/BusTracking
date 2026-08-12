namespace BusTracking.Common.Interfaces
{
    public interface IHomeworkService
    {
        Task<ApiResponse<HomeworkDto>> CreateHomeworkAsync(CreateHomeworkDto dto, int teacherUserId);
        Task<ApiResponse<HomeworkDto>> UpdateHomeworkAsync(UpdateHomeworkDto dto, int teacherUserId);
        Task<ApiResponse<List<HomeworkDto>>> GetHomeworksForTeacherAsync(int teacherUserId, int? academicYearId, int? standardId, int? sectionId);
        Task<ApiResponse<List<HomeworkDto>>> GetHomeworksForStudentAsync(int studentUserId, int? academicYearId, DateTime? fromDate = null, DateTime? toDate = null);

        Task<ApiResponse<HomeworkDto>> GetHomeworkByIdAsync(int homeworkId);
        Task<ApiResponse<bool>> SubmitHomeworkAsync(SubmitHomeworkDto dto, int studentUserId);
        Task<ApiResponse<List<HomeworkSubmissionDto>>> GetSubmissionsForHomeworkAsync(int homeworkId);
        Task<ApiResponse<bool>> EvaluateSubmissionAsync(EvaluateHomeworkSubmissionDto dto, int teacherUserId);
        Task<ApiResponse<bool>> DeleteHomeworkAsync(int homeworkId, int teacherUserId);
    }
}
