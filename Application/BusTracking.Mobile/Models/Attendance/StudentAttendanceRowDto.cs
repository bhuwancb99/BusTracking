namespace BusTracking.Mobile.Models.Attendance
{
    public class StudentAttendanceRowDto
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
        public string Status { get; set; } = "Present";
        public bool IsFaceScanned { get; set; }
        public double MatchConfidence { get; set; }
    }
}
