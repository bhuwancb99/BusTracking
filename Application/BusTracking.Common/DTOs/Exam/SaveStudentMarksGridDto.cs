namespace BusTracking.Common.DTOs.Exam
{
    public class SaveStudentMarksGridDto
    {
        [Required]
        public int ExamScheduleId { get; set; }

        public List<StudentMarkInputDto> Marks { get; set; } = new();
    }
}
