namespace BusTracking.Common.Interfaces
{
    public interface IExamService
    {
        // ── EXAM TERMS ──────────────────────────────────────────────────────
        Task<ApiResponse<List<ExamTermDto>>> GetExamTermsAsync(int? academicYearId);
        Task<ApiResponse<ExamTermDto>> CreateExamTermAsync(CreateExamTermDto dto);
        Task<ApiResponse<bool>> UpdateExamTermAsync(int examTermId, UpdateExamTermDto dto);
        Task<ApiResponse<bool>> DeleteExamTermAsync(int examTermId);


        // ── EXAM SCHEDULES (DATESHEET) ──────────────────────────────────────
        Task<ApiResponse<List<ExamScheduleDto>>> GetExamSchedulesAsync(int? examTermId, int? standardId);
        Task<ApiResponse<ExamScheduleDto>> CreateExamScheduleAsync(CreateExamScheduleDto dto);
        Task<ApiResponse<bool>> UpdateExamScheduleAsync(int examScheduleId, UpdateExamScheduleDto dto);
        Task<ApiResponse<bool>> DeleteExamScheduleAsync(int examScheduleId);


        // ── MARKS ENTRY GRID ────────────────────────────────────────────────
        Task<ApiResponse<List<StudentMarksGridItemDto>>> GetStudentMarksGridAsync(int examScheduleId, int? sectionId);
        Task<ApiResponse<bool>> SaveStudentMarksGridAsync(SaveStudentMarksGridDto dto, int teacherUserId);

        // ── REPORT CARD ─────────────────────────────────────────────────────
        Task<ApiResponse<StudentReportCardDto>> GetStudentReportCardAsync(int studentId, int examTermId);
    }
}
