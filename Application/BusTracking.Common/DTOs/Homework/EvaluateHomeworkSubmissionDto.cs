namespace BusTracking.Common.DTOs.Homework
{
    public class EvaluateHomeworkSubmissionDto
    {
        public int SubmissionId { get; set; }
        public string? TeacherRemarks { get; set; }
        public decimal? MarksObtained { get; set; }
        public string Status { get; set; } = "Reviewed";
    }
}
