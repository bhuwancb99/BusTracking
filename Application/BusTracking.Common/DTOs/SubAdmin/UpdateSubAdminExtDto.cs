namespace BusTracking.Common.DTOs.SubAdmin
{
    public class UpdateSubAdminExtDto
    {
        public string FullName { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; } = true;
        public List<int> PermissionIds { get; set; } = [];
    }
}
