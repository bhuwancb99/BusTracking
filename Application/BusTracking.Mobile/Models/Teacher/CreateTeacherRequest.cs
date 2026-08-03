namespace BusTracking.Mobile.Models.Teacher
{
    public class CreateTeacherRequest
    {
        public int? SchoolId { get; set; }
        public string FullName { get; set; } = "";
        public string UserName { get; set; } = "";
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string Password { get; set; } = "";
        public string? EmployeeCode { get; set; }
        public string? Qualification { get; set; }
        public string? Designation { get; set; }
        public string? Department { get; set; }
        public DateTime? JoiningDate { get; set; }
        public string? Gender { get; set; }
        public string? EmergencyContact { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
