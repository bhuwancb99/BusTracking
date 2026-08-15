namespace BusTracking.Mobile.Models.Exam
{
    public class StudentReportCardItem
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string RollNumber { get; set; } = string.Empty;
        public string StandardName { get; set; } = string.Empty;
        public string SectionName { get; set; } = string.Empty;
        public string TermName { get; set; } = string.Empty;
        public string AcademicYearName { get; set; } = string.Empty;
        public string SchoolName { get; set; } = string.Empty;
        public string? SchoolLogoUrl { get; set; }

        public decimal TotalMaxMarks { get; set; }
        public decimal TotalObtainedMarks { get; set; }
        public decimal TotalMarksObtained { get => TotalObtainedMarks; set => TotalObtainedMarks = value; }
        public decimal Percentage { get; set; }
        public string OverallGrade { get; set; } = string.Empty;
        public string ResultStatus { get; set; } = "PASS";

        [JsonPropertyName("SubjectMarks")]
        public List<ReportCardSubjectItem> SubjectMarks { get; set; } = [];

        [JsonIgnore]
        public List<ReportCardSubjectItem> Subjects
        {
            get => SubjectMarks;
            set => SubjectMarks = value;
        }

        public string RankDisplay => "Rank N/A";
        public string PercentageDisplay => $"{Percentage:F1}%";
        public Color StatusColor => ResultStatus.Equals("PASS", StringComparison.OrdinalIgnoreCase) || ResultStatus.Equals("PASSED", StringComparison.OrdinalIgnoreCase) ? Color.FromArgb("#22c55e") : Color.FromArgb("#ef4444");
    }
}
