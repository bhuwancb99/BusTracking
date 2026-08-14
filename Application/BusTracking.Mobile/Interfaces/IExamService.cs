namespace BusTracking.Mobile.Interfaces
{
    public interface IExamService
    {
        Task<List<ExamTermItem>> GetExamTermsAsync(int? academicYearId = null);
        Task<ApiResponse<object>> CreateExamTermAsync(CreateExamTermRequest req);
        Task<ApiResponse<object>> UpdateExamTermAsync(int examTermId, UpdateExamTermRequest req);
        Task<ApiResponse<object>> DeleteExamTermAsync(int examTermId);

        Task<List<ExamScheduleItem>> GetExamSchedulesAsync(int? examTermId = null, int? standardId = null);
        Task<ApiResponse<object>> CreateExamScheduleAsync(CreateExamScheduleRequest req);
        Task<ApiResponse<object>> UpdateExamScheduleAsync(int examScheduleId, UpdateExamScheduleRequest req);
        Task<ApiResponse<object>> DeleteExamScheduleAsync(int examScheduleId);

        Task<List<StudentMarksGridItem>> GetStudentMarksGridAsync(int examScheduleId, int? sectionId);
        Task<ApiResponse<object>> SaveStudentMarksGridAsync(SaveStudentMarksGridRequest req);
        Task<StudentReportCardItem?> GetStudentReportCardAsync(int examTermId, int? studentId = null);
    }
}
