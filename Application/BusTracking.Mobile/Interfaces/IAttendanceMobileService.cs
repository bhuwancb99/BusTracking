namespace BusTracking.Mobile.Interfaces
{
    public interface IAttendanceMobileService
    {
        Task<List<StudentAttendanceRowDto>> GetStudentsForAttendanceAsync(int academicYearId, int standardId, int? sectionId, DateTime date, bool isCoordinator = false, bool isAdmin = false);
        Task<ApiResponse<bool>> SaveManualAttendanceBatchAsync(ManualAttendanceBatchDto dto, bool isCoordinator = false, bool isAdmin = false);
        Task<ApiResponse<FaceAttendanceScanResultDto>> ProcessFaceScanAttendanceAsync(FaceAttendanceScanRequestDto dto, bool isCoordinator = false, bool isAdmin = false);
        Task<ApiResponse<AttendanceSummaryReportDto>> GetAttendanceReportAsync(int academicYearId, int standardId, int? sectionId, DateTime date, bool isCoordinator = false, bool isAdmin = false);
    }
}
