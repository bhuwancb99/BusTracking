namespace BusTracking.Common.DTOs.Student
{
    public class CreateStudentExtDto
    {
        [Required] public string FullName { get; set; } = "";
        [Required, MaxLength(100)] public string UserName { get; set; } = "";
        public string? Email { get; set; }
        public string StudentCode { get; set; } = "";
        public string? Password { get; set; }
        public string? Standard { get; set; }
        public int? BusId { get; set; }
        public int? StopId { get; set; }
        public bool SendWelcomeEmail { get; set; } = false;
    }
}
