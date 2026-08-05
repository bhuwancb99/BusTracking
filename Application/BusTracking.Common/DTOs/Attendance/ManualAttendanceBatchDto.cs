namespace BusTracking.Common.DTOs.Attendance
{
    public class ManualAttendanceBatchDto
    {
        public int AcademicYearId { get; set; }
        public int StandardId { get; set; }
        public int? SectionId { get; set; }
        public int? SubjectId { get; set; }
        public DateTime Date { get; set; }
        public DateTime AttendanceDate { get => Date; set => Date = value; }
        public List<StudentAttendanceItemDto> Items { get; set; } = new();
        public List<StudentAttendanceItemDto> Records { get => Items; set => Items = value; }
    }
}
