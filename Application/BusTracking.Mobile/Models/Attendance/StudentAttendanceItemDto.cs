namespace BusTracking.Mobile.Models.Attendance
{
    public class StudentAttendanceItemDto
    {
        public int StudentId { get; set; }
        public string Status { get; set; } = "Present";
        public bool IsFaceScanned { get; set; }
        public double? MatchConfidence { get; set; }
    }
}
