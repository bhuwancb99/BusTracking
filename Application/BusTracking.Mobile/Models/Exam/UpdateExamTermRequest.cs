namespace BusTracking.Mobile.Models.Exam
{
    public class UpdateExamTermRequest
    {
        public int AcademicYearId { get; set; }
        public string TermName { get; set; } = "";
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
    }
}
