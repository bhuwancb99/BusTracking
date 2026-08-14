namespace BusTracking.Common.DTOs.Exam
{
    public class StudentMarkInputDto
    {
        public int StudentId { get; set; }
        public decimal? MarksObtained { get; set; }
        public bool IsAbsent { get; set; }
        public string? Remarks { get; set; }
    }
}
