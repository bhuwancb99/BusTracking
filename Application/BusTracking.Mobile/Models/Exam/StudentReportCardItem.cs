namespace BusTracking.Mobile.Models.Exam
{
    public class StudentReportCardItem
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = "";
        public string RollNumber { get; set; } = "";
        public string StandardName { get; set; } = "";
        public string SectionName { get; set; } = "";
        public string TermName { get; set; } = "";
        public string AcademicYearName { get; set; } = "";
        public string SchoolName { get; set; } = "";
        public string? SchoolLogoUrl { get; set; }

        public decimal TotalMaxMarks { get; set; }
        public decimal TotalObtainedMarks { get; set; }
        public decimal Percentage { get; set; }
        public string OverallGrade { get; set; } = "";
        public string ResultStatus { get; set; } = ""; // PASSED / FAILED / PROMOTED
        public int ClassRank { get; set; }
        public int TotalStudentsInClass { get; set; }
        public string AttendanceDisplay { get; set; } = "";
        public string? TeacherRemarks { get; set; }

        public List<ReportCardSubjectItem> Subjects { get; set; } = [];

        public string RankDisplay => ClassRank > 0 ? $"Rank #{ClassRank} / {TotalStudentsInClass}" : "Rank N/A";
        public string PercentageDisplay => $"{Percentage:F1}%";
        public Color StatusColor => ResultStatus.Equals("PASSED", StringComparison.OrdinalIgnoreCase) ? Color.FromArgb("#22c55e") : Color.FromArgb("#ef4444");
    }
}
