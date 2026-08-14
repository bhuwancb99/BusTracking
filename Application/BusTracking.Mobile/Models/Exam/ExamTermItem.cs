namespace BusTracking.Mobile.Models.Exam
{
    public class ExamTermItem
    {
        public int ExamTermId { get; set; }
        public int AcademicYearId { get; set; }
        public string AcademicYearName { get; set; } = "";
        public string TermName { get; set; } = "";
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public int ScheduleCount { get; set; }

        public string DateRangeDisplay => $"{StartDate:MMM dd} - {EndDate:MMM dd, yyyy}";
    }
}
