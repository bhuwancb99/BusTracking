namespace BusTracking.Mobile.Models.Homework
{
    public class UpdateHomeworkDto
    {
        public int HomeworkId { get; set; }
        public int AcademicYearId { get; set; }
        public int StandardId { get; set; }
        public int? SectionId { get; set; }
        public int? SubjectId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? AttachmentUrl { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsActive { get; set; }
    }
}
