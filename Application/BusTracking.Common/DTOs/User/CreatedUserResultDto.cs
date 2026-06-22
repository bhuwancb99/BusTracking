namespace BusTracking.Common.DTOs.User
{
    public class CreatedUserResultDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = "";
        public string UserName { get; set; } = "";
        public string? Email { get; set; }
        public string PlainPassword { get; set; } = "";   // shown once in UI
        public string Role { get; set; } = "";
        public string GeneratedPassword { get; set; } = "";
        public bool EmailSent { get; set; }
    }
}
