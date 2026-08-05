namespace BusTracking.Common.DTOs.Attendance
{
    public class DailyAttendanceDto
    {
        public long AttendanceId { get; set; }
        public int AcademicYearId { get; set; }
        public int StandardId { get; set; }
        public string StandardName { get; set; } = string.Empty;
        public int? SectionId { get; set; }
        public string? SectionName { get; set; }
        public int? SubjectId { get; set; }
        public string? SubjectName { get; set; }
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public DateTime AttendanceDate { get; set; }
        public string Status { get; set; } = "Present";
        public bool IsFaceScanned { get; set; }
        public int? MarkedByUserId { get; set; }
        public string? MarkedByName { get; set; }
    }
}
