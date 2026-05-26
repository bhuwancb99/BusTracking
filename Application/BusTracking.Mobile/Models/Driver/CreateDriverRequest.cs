namespace BusTracking.Mobile.Models.Driver
{
    public class CreateDriverRequest
    {
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public string Password { get; set; } = "";
        public string? LicenseNumber { get; set; }
        public string? LicenseExpiry { get; set; }
        public int? BusId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
