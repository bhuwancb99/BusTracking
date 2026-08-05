namespace BusTracking.Mobile.Models.Attendance
{
    public class AttendanceSummaryReportDto
    {
        public int TotalStudents { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public double PresentPercentage { get; set; }
    }
}
