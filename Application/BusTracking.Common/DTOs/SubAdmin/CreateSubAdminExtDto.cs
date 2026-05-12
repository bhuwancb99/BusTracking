namespace BusTracking.Common.DTOs.SubAdmin
{
    public class CreateSubAdminExtDto
    {
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public List<int> PermissionIds { get; set; } = [];
        public bool SendWelcomeEmail { get; set; } = false;
    }
}
