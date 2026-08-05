namespace BusTracking.Common.DTOs.Attendance
{
    public class StudentAttendanceItemDto
    {
        public int StudentId { get; set; }
        public string Status { get; set; } = "Present";
    }
}
