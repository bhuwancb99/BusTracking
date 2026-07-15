namespace BusTracking.Common.DTOs.Parent
{
    public class LinkedStudentDto
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = "";
        public string FullName { get; set; } = "";
        public int? StandardId { get; set; }
        public string? StandardName { get; set; }
        public string? BusNumber { get; set; }
        public string? BusName { get; set; }
    }
}
