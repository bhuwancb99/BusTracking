namespace BusTracking.Common.DTOs.Exam
{
    public class UpdateExamScheduleDto
    {
        [Required]
        public int ExamScheduleId { get; set; }

        [Required]
        public int AcademicYearId { get; set; }

        [Required]
        public int ExamTermId { get; set; }

        [Required]
        public int StandardId { get; set; }

        public int? SectionId { get; set; }

        [Required]
        public int SubjectId { get; set; }

        [Required]
        public DateTime ExamDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        public decimal MaxMarks { get; set; } = 100;

        public decimal PassMarks { get; set; } = 33;

        public string? RoomNumber { get; set; }
    }
}
