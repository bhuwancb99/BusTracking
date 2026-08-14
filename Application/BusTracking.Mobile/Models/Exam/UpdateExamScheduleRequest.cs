namespace BusTracking.Mobile.Models.Exam
{
    public class UpdateExamScheduleRequest
    {
        public int ExamTermId { get; set; }
        public int StandardId { get; set; }
        public int? SectionId { get; set; }
        public int SubjectId { get; set; }
        public DateTime ExamDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal MaxMarks { get; set; }
        public decimal PassMarks { get; set; }
        public string? RoomNumber { get; set; }
    }
}
