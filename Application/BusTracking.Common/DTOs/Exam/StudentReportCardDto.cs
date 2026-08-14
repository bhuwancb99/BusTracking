namespace BusTracking.Common.DTOs.Exam
{
    public class StudentReportCardDto
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StandardName { get; set; } = string.Empty;
        public string SectionName { get; set; } = string.Empty;
        public string RollNumber { get; set; } = string.Empty;
        public int AcademicYearId { get; set; }
        public string AcademicYearName { get; set; } = string.Empty;
        public int ExamTermId { get; set; }
        public string TermName { get; set; } = string.Empty;

        public List<ReportCardSubjectMarkDto> SubjectMarks { get; set; } = new();

        public decimal TotalMaxMarks { get; set; }
        public decimal TotalMarksObtained { get; set; }
        public decimal Percentage { get; set; }
        public string OverallGrade { get; set; } = string.Empty;
        public string ResultStatus { get; set; } = "PASS"; // PASS / FAIL
    }
}
