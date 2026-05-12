namespace BusTracking.Common.DTOs.Student
{
    public class CreateStudentExtDto
    {
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string StudentCode { get; set; } = "";
        public string? Standard { get; set; }
        public int? BusId { get; set; }
        public int? StopId { get; set; }
        public bool SendWelcomeEmail { get; set; } = false;
    }

}
