namespace BusTracking.Mobile.Models.Homework
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
        public string Status { get; set; } = "Pending";
        public string? TeacherRemarks { get; set; }
        public decimal? MarksObtained { get; set; }
        public DateTime? EvaluatedAt { get; set; }

        public string StatusBadgeText => Status ?? "Pending";
        public string StatusBadgeBgColor => Status switch
        {
            "Evaluated" or "Reviewed" => "#16A34A",
            "Submitted" => "#2563EB",
            _ => "#EAB308"
        };
        public string StatusBadgeTextColor => Status switch
        {
            "Evaluated" or "Reviewed" or "Submitted" => "#FFFFFF",
            _ => "#000000"
        };
        public string StudentInitials
        {
            get
            {
                if (string.IsNullOrWhiteSpace(StudentName)) return "S";
                var parts = StudentName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 1) return parts[0][0].ToString().ToUpper();
                return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
            }
        }
    }
}
