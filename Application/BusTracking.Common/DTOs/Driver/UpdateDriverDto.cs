namespace BusTracking.Common.DTOs.Driver
{
    public class UpdateDriverDto
    {
        public string FullName { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public string? LicenseNumber { get; set; }
        public string? LicenseExpiry { get; set; }
        public int? BusId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
