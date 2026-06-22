namespace BusTracking.Common.DTOs.SubAdmin
{
    public class CreateSubAdminDto
    {
        [Required] public string FullName { get; set; } = "";
        [Required, MaxLength(100)] public string UserName { get; set; } = "";
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Password { get; set; }  // if null/empty → random generated
        public List<int> PermissionIds { get; set; } = [];
        public bool SendEmail { get; set; } = false;
    }
}
