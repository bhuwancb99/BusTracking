namespace BusTracking.Mobile.Models.Homework
{
    public class EvaluateHomeworkSubmissionDto
    {
        public int SubmissionId { get; set; }
        public decimal? MarksObtained { get; set; }
        public string? TeacherRemarks { get; set; }
        public string? Status { get; set; }
    }
}
