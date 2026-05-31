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
        public Color StatusBgColor => IsActive ? Color.FromArgb("#d1fae5") : Color.FromArgb("#f1f5f9");
        public Color StatusTextColor => IsActive ? Color.FromArgb("#065f46") : Color.FromArgb("#475569");
    }
}
