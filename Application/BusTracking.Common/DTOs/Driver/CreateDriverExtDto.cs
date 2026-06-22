namespace BusTracking.Common.DTOs.Driver
{
    public class CreateDriverExtDto
    {
        [Required] public string FullName { get; set; } = "";
        [Required, MaxLength(100)] public string UserName { get; set; } = "";
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Password { get; set; }
        public string? LicenseNumber { get; set; }
        public string? LicenseExpiry { get; set; }   // "yyyy-MM-dd"
        public int? BusId { get; set; }
        public bool SendWelcomeEmail { get; set; } = false;
    }
}
