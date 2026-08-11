namespace BusTracking.Common.DTOs.Homework
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
        public int TotalStudentsCount { get; set; }
    }
}
