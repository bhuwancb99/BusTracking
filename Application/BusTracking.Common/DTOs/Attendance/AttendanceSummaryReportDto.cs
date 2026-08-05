namespace BusTracking.Common.DTOs.Attendance
{
    public class AttendanceSummaryReportDto
    {
        public int TotalStudents { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public double PresentPercentage => TotalStudents > 0 ? Math.Round((double)PresentCount / TotalStudents * 100, 1) : 0;
        public List<DailyAttendanceDto> AttendanceList { get; set; } = new();
    }
}
