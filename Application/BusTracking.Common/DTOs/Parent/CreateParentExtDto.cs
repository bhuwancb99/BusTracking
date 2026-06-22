namespace BusTracking.Common.DTOs.Parent
{
    public class CreateParentExtDto
    {
        [Required] public string FullName { get; set; } = "";
        [Required, MaxLength(100)] public string UserName { get; set; } = "";
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Password { get; set; }
        public List<string> StudentCodes { get; set; } = [];
        public bool SendWelcomeEmail { get; set; } = false;
    }
}
