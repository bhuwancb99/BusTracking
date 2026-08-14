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

        public decimal TotalObtained => MarksObtained ?? ((TheoryMarks ?? 0) + (PracticalMarks ?? 0));
        public string StatusDisplay => IsAbsent ? "ABSENT" : $"{TotalObtained:0.#}";
        public Color StatusColor => IsAbsent ? Colors.Red : Colors.Green;
    }
}
