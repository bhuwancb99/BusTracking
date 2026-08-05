namespace BusTracking.Common.DTOs.Attendance
{
    public class StudentAttendanceRowDto
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
        public string Status { get; set; } = "Present"; // Present, Absent, Late, Excused
        public bool IsFaceScanned { get; set; }
        public double MatchConfidence { get; set; }
    }
}
