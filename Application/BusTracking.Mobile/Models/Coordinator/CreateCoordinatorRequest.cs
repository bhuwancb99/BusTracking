namespace BusTracking.Mobile.Models.Coordinator
{
    public class CreateCoordinatorRequest
    {
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public string Password { get; set; } = "";
        public List<int> PermissionIds { get; set; } = [];
        public bool IsActive { get; set; } = true;
    }
}
