namespace BusTracking.Mobile.Models.Exam
{
    public class ReportCardSubjectItem
    {
        public string SubjectName { get; set; } = string.Empty;
        public decimal MaxMarks { get; set; }
        public decimal PassMarks { get; set; }
        public decimal? MarksObtained { get; set; }
        public string Grade { get; set; } = string.Empty;
        public bool IsAbsent { get; set; }
        public string Remarks { get; set; } = string.Empty;

        public bool IsPass => !IsAbsent && MarksObtained.HasValue && MarksObtained.Value >= PassMarks;
        public string StatusDisplay => IsAbsent ? "ABSENT" : (IsPass ? "PASS" : "FAIL");
        public Color StatusColor => IsAbsent || !IsPass ? Color.FromArgb("#ef4444") : Color.FromArgb("#22c55e");
        public string DisplayMarks => IsAbsent ? "ABSENT" : $"{(MarksObtained.HasValue ? MarksObtained.Value.ToString("0.##") : "N/A")} / {MaxMarks:0.##}";
    }
}
