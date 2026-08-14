namespace BusTracking.Common.DTOs.Exam
{
    public class UpdateExamTermDto
    {
        [Required]
        public int ExamTermId { get; set; }

        [Required]
        public int AcademicYearId { get; set; }

        [Required, MaxLength(150)]
        public string TermName { get; set; } = string.Empty;


        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
