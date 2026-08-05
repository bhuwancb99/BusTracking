namespace BusTracking.Mobile.Models.Attendance
{
    public class FaceAttendanceScanRequestDto
    {
        public int AcademicYearId { get; set; }
        public int StandardId { get; set; }
        public int? SectionId { get; set; }
        public int? SubjectId { get; set; }
        public DateTime Date { get; set; }
        public DateTime AttendanceDate { get => Date; set => Date = value; }
        public string Base64Image { get; set; } = string.Empty;
    }
}
