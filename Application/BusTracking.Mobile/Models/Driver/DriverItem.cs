namespace BusTracking.Mobile.Models.Driver
{
    public class DriverItem
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public int? BusId { get; set; }
        public string? BusName { get; set; }
        public string? BusNumber { get; set; }
        public string? LicenseNumber { get; set; }
        public string? LicenseExpiry { get; set; }
        public bool IsActive { get; set; }

        public string StatusLabel => IsActive ? "Active" : "Inactive";
        public Color StatusColor => IsActive ? Colors.Green : Colors.Red;
        public string BusDisplay => BusName != null ? $"{BusName} ({BusNumber})" : "No bus assigned";
        public string InitialsDisplay => FullName.Length >= 2 ? FullName[..2].ToUpper() : FullName.ToUpper();
    }
}
