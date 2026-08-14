namespace BusTracking.Mobile.Models.Exam
{
    public class ReportCardSubjectItem
    {
        public string SubjectName { get; set; } = "";
        public decimal MaxMarks { get; set; }
        public decimal PassMarks { get; set; }
        public decimal TheoryMarks { get; set; }
        public decimal PracticalMarks { get; set; }
        public decimal TotalMarks { get; set; }
        public string Grade { get; set; } = "";
        public bool IsAbsent { get; set; }

        public string DisplayMarks => IsAbsent ? "ABS" : $"{TotalMarks:0.#} / {MaxMarks:0.#}";
    }
}
