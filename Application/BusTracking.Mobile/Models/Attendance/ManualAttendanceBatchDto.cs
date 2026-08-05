namespace BusTracking.Mobile.Models.Attendance
{
    public class ManualAttendanceBatchDto
    {
        public int AcademicYearId { get; set; }
        public int StandardId { get; set; }
        public int? SectionId { get; set; }
        public int? SubjectId { get; set; }
        public DateTime Date { get; set; }
        public List<StudentAttendanceItemDto> Items { get; set; } = new();
    }
}
