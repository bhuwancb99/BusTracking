namespace BusTracking.Mobile.Interfaces
{
    public interface IAttendanceMobileService
    {
        Task<List<StudentAttendanceRowDto>> GetStudentsForAttendanceAsync(int academicYearId, int standardId, int? sectionId, DateTime date);
        Task<ApiResponse<bool>> SaveManualAttendanceBatchAsync(ManualAttendanceBatchDto dto);
        Task<ApiResponse<FaceAttendanceScanResultDto>> ProcessFaceScanAttendanceAsync(FaceAttendanceScanRequestDto dto);
        Task<ApiResponse<AttendanceSummaryReportDto>> GetAttendanceReportAsync(int academicYearId, int standardId, int? sectionId, DateTime date);
    }
}
