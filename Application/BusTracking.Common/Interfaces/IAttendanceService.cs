namespace BusTracking.Common.Interfaces
{
    public interface IAttendanceService
    {
        Task<ApiResponse<List<StudentAttendanceRowDto>>> GetStudentsForAttendanceAsync(int academicYearId, int standardId, int? sectionId, DateTime date);
        Task<ApiResponse<bool>> SaveManualAttendanceBatchAsync(ManualAttendanceBatchDto dto, int markedByUserId);
        Task<ApiResponse<FaceAttendanceScanResultDto>> ProcessFaceScanAttendanceAsync(FaceAttendanceScanRequestDto dto, int markedByUserId);
        Task<ApiResponse<AttendanceSummaryReportDto>> GetAttendanceReportAsync(int academicYearId, int standardId, int? sectionId, DateTime date);
    }
}
