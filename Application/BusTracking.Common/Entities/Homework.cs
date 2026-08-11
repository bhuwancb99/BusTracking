namespace BusTracking.Common.Entities
{
    public class Homework : IMultiTenant
    {
        public int? SchoolId { get; set; }

        [Key]
        public int HomeworkId { get; set; }
        public int AcademicYearId { get; set; }
        public int StandardId { get; set; }
        public int? SectionId { get; set; }
        public int? SubjectId { get; set; }
        public int TeacherUserId { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? AttachmentUrl { get; set; }

        public DateTime DueDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        [ForeignKey(nameof(AcademicYearId))]
        public virtual AcademicYear? AcademicYear { get; set; }

        [ForeignKey(nameof(StandardId))]
        public virtual StandardMaster? Standard { get; set; }

        [ForeignKey(nameof(SectionId))]
        public virtual Section? Section { get; set; }

        [ForeignKey(nameof(SubjectId))]
        public virtual SubjectMaster? Subject { get; set; }

        [ForeignKey(nameof(TeacherUserId))]
        public virtual User? TeacherUser { get; set; }

        public virtual ICollection<HomeworkSubmission> Submissions { get; set; } = new List<HomeworkSubmission>();
    }
}
