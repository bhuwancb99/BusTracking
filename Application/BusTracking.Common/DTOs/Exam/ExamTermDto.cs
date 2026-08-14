namespace BusTracking.Common.DTOs.Exam
{
    public class ExamTermDto
    {
        public int ExamTermId { get; set; }
        public int AcademicYearId { get; set; }
        public string AcademicYearName { get; set; } = string.Empty;
        public string TermName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public int ScheduleCount { get; set; }
    }
}
