namespace BusTracking.Mobile.Models.User
{
    public class UserProfileDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = "";
        public string UserName { get; set; } = "";
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string Role { get; set; } = "";
        public bool IsActive { get; set; }
        public int? SchoolId { get; set; }
        public string? SchoolName { get; set; }
        public string? SchoolLogo { get; set; }
    }
}
