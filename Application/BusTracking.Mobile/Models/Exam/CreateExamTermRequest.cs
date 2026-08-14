namespace BusTracking.Mobile.Models.Exam
{
    public class CreateExamTermRequest
    {
        public int AcademicYearId { get; set; }
        public string TermName { get; set; } = "";
        public string? Description { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Today;
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(14);
        public bool IsActive { get; set; } = true;
    }
}
