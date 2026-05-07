namespace BusTracking.Common.Models
{
    public class PermissionCheckResult
    {
        public bool HasPermission { get; set; }
        public string PermissionKey { get; set; } = "";
        public string Message { get; set; } = "";
    }
}
