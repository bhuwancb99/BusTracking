namespace BusTracking.Common.DTOs.SubAdmin
{
    public class SubAdminListDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = "";
        public string UserName { get; set; } = "";
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public List<string> Permissions { get; set; } = [];
        public DateTime CreatedAt { get; set; }
        public string? ProfileImageUrl { get; set; }
    }
}
