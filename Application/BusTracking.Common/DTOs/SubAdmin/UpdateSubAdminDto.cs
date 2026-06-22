namespace BusTracking.Common.DTOs.SubAdmin
{
    public class UpdateSubAdminDto
    {
        [Required] public string FullName { get; set; } = "";
        [Required, MaxLength(100)] public string UserName { get; set; } = "";
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? NewPassword { get; set; }  // if null/empty → keep existing
        public bool IsActive { get; set; } = true;
        public List<int> PermissionIds { get; set; } = [];
    }
}
