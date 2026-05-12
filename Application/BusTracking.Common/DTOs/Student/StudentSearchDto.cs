namespace BusTracking.Common.DTOs.Student
{
    public class StudentSearchDto
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = "";
        public string FullName { get; set; } = "";
        public string? Standard { get; set; }
        public string? BusNumber { get; set; }
    }
}
