namespace BusTracking.Mobile.Models.Exam
{
    public class CreateExamScheduleRequest
    {
        public int AcademicYearId { get; set; }
        public int ExamTermId { get; set; }
        public int StandardId { get; set; }
        public int? SectionId { get; set; }
        public int SubjectId { get; set; }
        public DateTime ExamDate { get; set; } = DateTime.Today;
        public TimeSpan StartTime { get; set; } = new TimeSpan(9, 0, 0);
        public TimeSpan EndTime { get; set; } = new TimeSpan(12, 0, 0);
        public decimal MaxMarks { get; set; } = 100;
        public decimal PassMarks { get; set; } = 33;
        public string? RoomNumber { get; set; }
    }
}
