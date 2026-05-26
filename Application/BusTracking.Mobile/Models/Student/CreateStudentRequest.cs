namespace BusTracking.Mobile.Models.Student
{
    public class CreateStudentRequest
    {
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public string Password { get; set; } = "";
        public string? Standard { get; set; }
        public int? BusId { get; set; }
        public int? StopId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
