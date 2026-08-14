namespace BusTracking.Common.Entities
{
    [Table("ExamSchedules")]
    public class ExamSchedule : IMultiTenant
    {
        [Key]
        public int ExamScheduleId { get; set; }

        public int? SchoolId { get; set; }


        [Required]
        public int AcademicYearId { get; set; }

        [Required]
        public int ExamTermId { get; set; }

        [Required]
        public int StandardId { get; set; }

        public int? SectionId { get; set; }

        [Required]
        public int SubjectId { get; set; }


        public DateTime ExamDate { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MaxMarks { get; set; } = 100;

        [Column(TypeName = "decimal(18,2)")]
        public decimal PassMarks { get; set; } = 33;

        [MaxLength(50)]
        public string? RoomNumber { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("SchoolId")]
        public virtual School? School { get; set; }

        [ForeignKey("AcademicYearId")]
        public virtual AcademicYear? AcademicYear { get; set; }

        [ForeignKey("ExamTermId")]
        public virtual ExamTerm? ExamTerm { get; set; }

        [ForeignKey("StandardId")]
        public virtual StandardMaster? Standard { get; set; }

        [ForeignKey("SectionId")]
        public virtual Section? Section { get; set; }

        [ForeignKey("SubjectId")]
        public virtual SubjectMaster? Subject { get; set; }

    }
}
