namespace BusTracking.Mobile.Models.Coordinator
{
    public class UpdateCoordinatorRequest
    {
        public string FullName { get; set; } = "";
        public string UserName { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public List<int> PermissionIds { get; set; } = [];
        public bool IsActive { get; set; } = true;
    }
}
