namespace BusTracking.Common.DTOs.Parent
{
    public class CreateParentDto
    {
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public List<string> StudentCodes { get; set; } = [];
        public bool SendEmail { get; set; } = false;
    }
}
