namespace BusTracking.API.Models
{
    public class DriverProfileModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? LicenseNumber { get; set; }
        public string? LicenseExpiry { get; set; }   // "yyyy-MM-dd"
        public int? BusId { get; set; }
        public string? BusName { get; set; }
        public string? BusNumber { get; set; }
        public string? RouteName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
