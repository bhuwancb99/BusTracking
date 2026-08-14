namespace BusTracking.Mobile.Models.Exam
{
    public class SaveStudentMarksItemRequest
    {
        public int StudentId { get; set; }
        public decimal? TheoryMarks { get; set; }
        public decimal? PracticalMarks { get; set; }
        public bool IsAbsent { get; set; }
        public string? Remarks { get; set; }
    }
}
