namespace BusTracking.Common.Entities
{
    [Table("ExamMarks")]
    public class ExamMark : IMultiTenant
    {
        [Key]
        public int ExamMarkId { get; set; }

        public int? SchoolId { get; set; }


        [Required]
        public int ExamScheduleId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MarksObtained { get; set; }

        [MaxLength(10)]
        public string? Grade { get; set; }

        public bool IsAbsent { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }

        public int? EvaluatedByTeacherUserId { get; set; }

        public DateTime? EvaluatedAt { get; set; }

        [ForeignKey("SchoolId")]
        public virtual School? School { get; set; }

        [ForeignKey("ExamScheduleId")]
        public virtual ExamSchedule? ExamSchedule { get; set; }

        [ForeignKey("StudentId")]
        public virtual StudentDetail? Student { get; set; }

        [ForeignKey("EvaluatedByTeacherUserId")]
        public virtual User? EvaluatedByTeacherUser { get; set; }
    }
}
