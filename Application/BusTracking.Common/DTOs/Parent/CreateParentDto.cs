namespace BusTracking.Common.DTOs.Parent
{
    public class CreateParentDto
    {
        [Required] public string FullName { get; set; } = "";
        [Required, MaxLength(100)] public string UserName { get; set; } = "";
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Password { get; set; }
        public List<string> StudentCodes { get; set; } = [];
        public bool SendEmail { get; set; } = false;
    }
}
