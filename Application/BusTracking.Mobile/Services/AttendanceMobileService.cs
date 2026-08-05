namespace BusTracking.Mobile.Services
{
    public class AttendanceMobileService : IAttendanceMobileService
    {
        private readonly IApiService _api;

        public AttendanceMobileService(IApiService api)
        {
            _api = api;
        }

        public async Task<List<StudentAttendanceRowDto>> GetStudentsForAttendanceAsync(int academicYearId, int standardId, int? sectionId, DateTime date)
        {
            var url = $"{Constants.Teacher.AttendanceStudents}?academicYearId={academicYearId}&standardId={standardId}&date={date:yyyy-MM-dd}";
            if (sectionId.HasValue && sectionId.Value > 0) url += $"&sectionId={sectionId.Value}";

            var res = await _api.GetAsync<List<StudentAttendanceRowDto>>(url);
            return res.Data ?? new List<StudentAttendanceRowDto>();
        }

        public async Task<ApiResponse<bool>> SaveManualAttendanceBatchAsync(ManualAttendanceBatchDto dto)
        {
            var res = await _api.PostAsync<bool>(Constants.Teacher.AttendanceManualBatch, dto);
            return res ?? ApiResponse<bool>.Fail("Failed to save manual attendance batch.");
        }

        public async Task<ApiResponse<FaceAttendanceScanResultDto>> ProcessFaceScanAttendanceAsync(FaceAttendanceScanRequestDto dto)
        {
            var res = await _api.PostAsync<FaceAttendanceScanResultDto>(Constants.Teacher.AttendanceFaceScan, dto);
            return res ?? ApiResponse<FaceAttendanceScanResultDto>.Fail("Failed to process face scan attendance.");
        }

        public async Task<ApiResponse<AttendanceSummaryReportDto>> GetAttendanceReportAsync(int academicYearId, int standardId, int? sectionId, DateTime date)
        {
            var url = $"{Constants.Teacher.AttendanceReport}?academicYearId={academicYearId}&standardId={standardId}&date={date:yyyy-MM-dd}";
            if (sectionId.HasValue && sectionId.Value > 0) url += $"&sectionId={sectionId.Value}";

            var res = await _api.GetAsync<AttendanceSummaryReportDto>(url);
            return res ?? ApiResponse<AttendanceSummaryReportDto>.Fail("Failed to fetch attendance report.");
        }
    }
}
