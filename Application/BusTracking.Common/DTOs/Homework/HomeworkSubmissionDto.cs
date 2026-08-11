namespace BusTracking.Common.DTOs.Homework
{
    public class HomeworkSubmissionDto
    {
        public int SubmissionId { get; set; }
        public int HomeworkId { get; set; }
        public string HomeworkTitle { get; set; } = string.Empty;
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
        public string? SubmissionText { get; set; }
        public string? AttachmentUrl { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? TeacherRemarks { get; set; }
        public decimal? MarksObtained { get; set; }
        public DateTime? EvaluatedAt { get; set; }
    }
}
