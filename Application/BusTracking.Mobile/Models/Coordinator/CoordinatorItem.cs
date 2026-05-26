namespace BusTracking.Mobile.Models.Coordinator
{
    public class CoordinatorItem
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public List<string> Permissions { get; set; } = [];
        public DateTime CreatedAt { get; set; }

        public string StatusLabel => IsActive ? "Active" : "Inactive";
        public Color StatusColor => IsActive ? Colors.Green : Colors.Red;
        public string PermCount => $"{Permissions.Count} permissions";
        public string InitialsDisplay => FullName.Length >= 2 ? FullName[..2].ToUpper() : FullName.ToUpper();
    }
}
