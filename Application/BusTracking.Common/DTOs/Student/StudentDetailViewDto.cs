namespace BusTracking.Common.DTOs.Student
{
    public class StudentDetailViewDto
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = "";
        public string FullName { get; set; } = "";
        public string UserName { get; set; } = "";
        public string? Email { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public int? StandardId { get; set; }
        public string? StandardName { get; set; }
        public int? BusId { get; set; }
        public string? BusName { get; set; }
        public string? BusNumber { get; set; }
        public int? StopId { get; set; }
        public string? StopName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> ParentNames { get; set; } = [];
    }

}
