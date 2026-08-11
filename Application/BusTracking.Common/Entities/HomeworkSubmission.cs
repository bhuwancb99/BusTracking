namespace BusTracking.Common.Entities
{
    public class HomeworkSubmission
    {
        [Key]
        public int SubmissionId { get; set; }
        public int HomeworkId { get; set; }
        public int StudentId { get; set; }

        public string? SubmissionText { get; set; }

        [MaxLength(500)]
        public string? AttachmentUrl { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        public string Status { get; set; } = "Submitted"; // Submitted, Reviewed

        public string? TeacherRemarks { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? MarksObtained { get; set; }

        public DateTime? EvaluatedAt { get; set; }

        [ForeignKey(nameof(HomeworkId))]
        public virtual Homework? Homework { get; set; }

        [ForeignKey(nameof(StudentId))]
        public virtual StudentDetail? Student { get; set; }
    }
}
