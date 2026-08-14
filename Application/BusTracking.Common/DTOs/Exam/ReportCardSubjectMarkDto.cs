namespace BusTracking.Common.DTOs.Exam
{
    public class ReportCardSubjectMarkDto
    {
        public string SubjectName { get; set; } = string.Empty;
        public decimal MaxMarks { get; set; }
        public decimal PassMarks { get; set; }
        public decimal? MarksObtained { get; set; }
        public string Grade { get; set; } = string.Empty;
        public bool IsAbsent { get; set; }
        public string Remarks { get; set; } = string.Empty;
    }
}
