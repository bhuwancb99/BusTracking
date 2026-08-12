namespace BusTracking.Mobile.Models.Homework
{
    public class HomeworkDto
    {
        public int HomeworkId { get; set; }
        public int AcademicYearId { get; set; }
        public string YearName { get; set; } = string.Empty;
        public int StandardId { get; set; }
        public string StandardName { get; set; } = string.Empty;
        public int? SectionId { get; set; }
        public string? SectionName { get; set; }
        public int? SubjectId { get; set; }
        public string? SubjectName { get; set; }
        public int TeacherUserId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? AttachmentUrl { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public int SubmissionsCount { get; set; }

        public bool IsSubmitted { get; set; }
        public string? SubmissionStatus { get; set; }
        public string? TeacherRemarks { get; set; }
        public decimal? MarksObtained { get; set; }

        public bool IsPending => !IsSubmitted;
        public bool HasAttachment => !string.IsNullOrWhiteSpace(AttachmentUrl);
        public string StatusBadgeText => IsActive ? "Active" : "Draft";
        public string StatusBadgeBgColor => IsActive ? "#16A34A" : "#6B7280";

        public string StudentBadgeText => IsSubmitted ? "Submitted" : $"Due: {DueDate:dd MMM yyyy}";

        public string StudentBadgeBgColor => IsSubmitted ? "#DCFCE7" : "#FEF3C7";
        public string StudentBadgeTextColor => IsSubmitted ? "#15803D" : "#B45309";
    }
}
