namespace BusTracking.Common.DTOs.Auth
{
    public class LoginResponseDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = "";
        public string UserName { get; set; } = "";
        public string? Email { get; set; }
        public string Role { get; set; } = "";
        public string Token { get; set; } = "";
        public DateTime Expiry { get; set; }
        public string Permissions { get; set; } = "";
        public int? SchoolId { get; set; }
        public string? SchoolName { get; set; }
        public string? TimeZoneInfoId { get; set; } = "India Standard Time";
        public string? TimeZoneName { get; set; }
        public string? UtcOffset { get; set; }
    }
}
