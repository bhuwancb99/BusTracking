namespace BusTracking.Common.DTOs.Exam
{
    public class ExamScheduleDto
    {
        public int ExamScheduleId { get; set; }
        public int ExamTermId { get; set; }
        public string TermName { get; set; } = string.Empty;
        public int StandardId { get; set; }
        public string StandardName { get; set; } = string.Empty;
        public int? SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public DateTime ExamDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal MaxMarks { get; set; }
        public decimal PassMarks { get; set; }
        public string? RoomNumber { get; set; }
    }
}
