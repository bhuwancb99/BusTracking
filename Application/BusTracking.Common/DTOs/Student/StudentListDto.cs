namespace BusTracking.Common.DTOs.Student
{
    public class StudentListDto
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? Standard { get; set; }
        public string? BusNumber { get; set; }
        public string? StopName { get; set; }
        public bool IsActive { get; set; }
    }
}
