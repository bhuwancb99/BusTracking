namespace BusTracking.Mobile.Models.Student
{
    public class UpdateStudentRequest
    {
        public string FullName { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public string? Standard { get; set; }
        public int? BusId { get; set; }
        public int? StopId { get; set; }
        public List<string> StudentCodes { get; set; } = [];
        public bool IsActive { get; set; } = true;
    }
}
