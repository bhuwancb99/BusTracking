namespace BusTracking.Common.DTOs.Parent
{
    public class UpdateParentExtDto
    {
        public string FullName { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; } = true;
        public List<string> StudentCodes { get; set; } = [];
    }
}
