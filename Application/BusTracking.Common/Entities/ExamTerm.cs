namespace BusTracking.Common.Entities
{
    [Table("ExamTerms")]
    public class ExamTerm : IMultiTenant
    {
        [Key]
        public int ExamTermId { get; set; }

        public int? SchoolId { get; set; }


        [Required]
        public int AcademicYearId { get; set; }

        [Required, MaxLength(150)]
        public string TermName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("SchoolId")]
        public virtual School? School { get; set; }

        [ForeignKey("AcademicYearId")]
        public virtual AcademicYear? AcademicYear { get; set; }
    }
}
