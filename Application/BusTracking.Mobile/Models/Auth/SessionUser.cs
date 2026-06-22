using SQLite;

namespace BusTracking.Mobile.Models.Auth
{
    /// <summary>Stored encrypted in SQLite after login</summary>
    [Table("SessionUser")]
    public class SessionUser
    {
        [PrimaryKey, AutoIncrement] public int Id { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = "";
        public string UserName { get; set; } = "";
        public string? Email { get; set; }
        public string Role { get; set; } = "";
        public string Token { get; set; } = "";   // stored encrypted
        public DateTime Expiry { get; set; }
        public string Permissions { get; set; } = ""; // JSON array of permission keys
        public string? ProfileImageUrl { get; set; }  // full URL from API server
    }
}
