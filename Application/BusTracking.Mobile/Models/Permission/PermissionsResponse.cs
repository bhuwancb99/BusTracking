namespace BusTracking.Mobile.Models.Permission
{
    public class PermissionsResponse
    {
        public List<int> AssignedPermissionIds { get; set; } = [];
        public List<PermissionEntry> AllPermissions { get; set; } = [];
    }
}
