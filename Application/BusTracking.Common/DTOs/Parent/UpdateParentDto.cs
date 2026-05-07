namespace BusTracking.Common.DTOs.Parent
{
    public class UpdateParentDto
    {
        public string FullName { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public List<string> StudentCodes { get; set; } = [];
    }
}
