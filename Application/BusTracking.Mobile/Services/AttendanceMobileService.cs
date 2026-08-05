namespace BusTracking.Mobile.Services
{
    public class AttendanceMobileService : IAttendanceMobileService
    {
        private readonly IApiService _api;

        public AttendanceMobileService(IApiService api)
        {
            _api = api;
        }

        public async Task<List<StudentAttendanceRowDto>> GetStudentsForAttendanceAsync(int academicYearId, int standardId, int? sectionId, DateTime date, bool isCoordinator = false, bool isAdmin = false)
        {
            var baseEndpoint = isAdmin ? Constants.Admin.AttendanceStudents :
                               isCoordinator ? Constants.Coordinator.AttendanceStudents :
                               Constants.Teacher.AttendanceStudents;

            var url = $"{baseEndpoint}?academicYearId={academicYearId}&standardId={standardId}&date={date:yyyy-MM-dd}";
            if (sectionId.HasValue && sectionId.Value > 0) url += $"&sectionId={sectionId.Value}";

            var res = await _api.GetAsync<List<StudentAttendanceRowDto>>(url);
            return res.Data ?? new List<StudentAttendanceRowDto>();
        }

        public async Task<ApiResponse<bool>> SaveManualAttendanceBatchAsync(ManualAttendanceBatchDto dto, bool isCoordinator = false, bool isAdmin = false)
        {
            var endpoint = isAdmin ? Constants.Admin.AttendanceManualBatch :
                           isCoordinator ? Constants.Coordinator.AttendanceManualBatch :
                           Constants.Teacher.AttendanceManualBatch;

            var res = await _api.PostAsync<bool>(endpoint, dto);
            return res ?? ApiResponse<bool>.Fail("Failed to save manual attendance batch.");
        }

        public async Task<ApiResponse<FaceAttendanceScanResultDto>> ProcessFaceScanAttendanceAsync(FaceAttendanceScanRequestDto dto, bool isCoordinator = false, bool isAdmin = false)
        {
            var endpoint = isAdmin ? Constants.Admin.AttendanceFaceScan :
                           isCoordinator ? Constants.Coordinator.AttendanceFaceScan :
                           Constants.Teacher.AttendanceFaceScan;

            var res = await _api.PostAsync<FaceAttendanceScanResultDto>(endpoint, dto);
            return res ?? ApiResponse<FaceAttendanceScanResultDto>.Fail("Failed to process face scan attendance.");
        }

        public async Task<ApiResponse<AttendanceSummaryReportDto>> GetAttendanceReportAsync(int academicYearId, int standardId, int? sectionId, DateTime date, bool isCoordinator = false, bool isAdmin = false)
        {
            var baseEndpoint = isAdmin ? Constants.Admin.AttendanceReport :
                               isCoordinator ? Constants.Coordinator.AttendanceReport :
                               Constants.Teacher.AttendanceReport;

            var url = $"{baseEndpoint}?academicYearId={academicYearId}&standardId={standardId}&date={date:yyyy-MM-dd}";
            if (sectionId.HasValue && sectionId.Value > 0) url += $"&sectionId={sectionId.Value}";

            var res = await _api.GetAsync<AttendanceSummaryReportDto>(url);
            return res ?? ApiResponse<AttendanceSummaryReportDto>.Fail("Failed to fetch attendance report.");
        }
    }
}
