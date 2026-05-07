namespace BusTracking.Common.DTOs.Driver
{
    public class DriverListDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public string? BusNumber { get; set; }
        public string? BusName { get; set; }
        public string? LicenseNumber { get; set; }
        public bool IsActive { get; set; }
    }
}
