namespace BusTracking.Common.DTOs.SubAdmin
{
    public class CreateSubAdminExtDto
    {
        [Required] public string FullName { get; set; } = "";
        [Required, MaxLength(100)] public string UserName { get; set; } = "";
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Password { get; set; }
        public List<int> PermissionIds { get; set; } = [];
        public bool SendWelcomeEmail { get; set; } = false;
    }
}
