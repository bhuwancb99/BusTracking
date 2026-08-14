namespace BusTracking.Common.DTOs.Exam
{
    public class StudentMarksGridItemDto
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string RollNumber { get; set; } = string.Empty;
        public int? ExamMarkId { get; set; }
        public decimal? MarksObtained { get; set; }
        public string? Grade { get; set; }
        public bool IsAbsent { get; set; }
        public string? Remarks { get; set; }
    }
}
