namespace BusTracking.Common.DTOs.Student
{
    public class UpdateStudentDto
    {
        public string FullName { get; set; } = "";
        public string? Standard { get; set; }
        public int? BusId { get; set; }
        public int? StopId { get; set; }
    }
}
