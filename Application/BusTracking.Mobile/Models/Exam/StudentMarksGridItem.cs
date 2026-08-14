namespace BusTracking.Mobile.Models.Exam
{
    public partial class StudentMarksGridItem : ObservableObject
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = "";
        public string StudentName { get; set; } = "";
        public int RollNumber { get; set; }
        public int? ExamMarksId { get; set; }

        [ObservableProperty] private decimal? _theoryMarks;
        [ObservableProperty] private decimal? _practicalMarks;
        [ObservableProperty] private bool _isAbsent;
        [ObservableProperty] private string? _remarks;

        public decimal TotalObtained => (TheoryMarks ?? 0) + (PracticalMarks ?? 0);
        public string StatusDisplay => IsAbsent ? "ABSENT" : $"{TotalObtained:0.#}";
        public Color StatusColor => IsAbsent ? Colors.Red : Colors.Green;
    }
}
