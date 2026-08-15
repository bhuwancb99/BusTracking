namespace BusTracking.Mobile.Models.Exam
{
    public partial class StudentMarksGridItem : ObservableObject
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string RollNumber { get; set; } = string.Empty;
        public int? ExamMarkId { get; set; }

        [ObservableProperty] private decimal? _marksObtained;
        [ObservableProperty] private decimal? _theoryMarks;
        [ObservableProperty] private decimal? _practicalMarks;
        [ObservableProperty] private string? _grade;
        [ObservableProperty] private bool _isAbsent;
        [ObservableProperty] private string? _remarks;

        [ObservableProperty] private Color _gradeColor = Color.FromArgb("#0284c7");
        [ObservableProperty] private decimal _maxMarks = 100;

        public decimal TotalObtained => MarksObtained ?? ((TheoryMarks ?? 0) + (PracticalMarks ?? 0));
        public string StatusDisplay => IsAbsent ? "ABSENT" : $"{TotalObtained:0.#}";
        public Color StatusColor => IsAbsent ? Colors.Red : Colors.Green;

        partial void OnMarksObtainedChanged(decimal? value) => RecalculateGrade();
        partial void OnIsAbsentChanged(bool value) => RecalculateGrade();
        partial void OnTheoryMarksChanged(decimal? value) => RecalculateGrade();
        partial void OnPracticalMarksChanged(decimal? value) => RecalculateGrade();
        partial void OnMaxMarksChanged(decimal value) => RecalculateGrade();

        public void RecalculateGrade()
        {
            if (IsAbsent)
            {
                Grade = "ABS";
                GradeColor = Color.FromArgb("#dc2626"); // Red
                return;
            }

            if (!MarksObtained.HasValue && !TheoryMarks.HasValue && !PracticalMarks.HasValue)
            {
                Grade = "-";
                GradeColor = Color.FromArgb("#94a3b8"); // Gray
                return;
            }

            decimal total = TotalObtained;
            decimal max = MaxMarks > 0 ? MaxMarks : 100;
            double pct = (double)(total / max) * 100.0;

            if (pct >= 90)
            {
                Grade = "A+";
                GradeColor = Color.FromArgb("#16a34a"); // Green
            }
            else if (pct >= 80)
            {
                Grade = "A";
                GradeColor = Color.FromArgb("#2563eb"); // Primary Blue
            }
            else if (pct >= 70)
            {
                Grade = "B";
                GradeColor = Color.FromArgb("#0284c7"); // Info Sky Blue
            }
            else if (pct >= 60)
            {
                Grade = "C";
                GradeColor = Color.FromArgb("#d97706"); // Warning Orange
            }
            else if (pct >= 33)
            {
                Grade = "D";
                GradeColor = Color.FromArgb("#64748b"); // Slate Gray
            }
            else
            {
                Grade = "F";
                GradeColor = Color.FromArgb("#dc2626"); // Danger Red
            }
        }
    }
}
