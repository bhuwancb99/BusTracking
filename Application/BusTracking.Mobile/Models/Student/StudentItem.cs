namespace BusTracking.Mobile.Models.Student
{
    public class StudentItem
    {
        public int StudentId { get; set; }
        public int UserId { get; set; }
        public string StudentCode { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public string? Standard { get; set; }
        public int? BusId { get; set; }
        public string? BusName { get; set; }
        public string? BusNumber { get; set; }
        public int? StopId { get; set; }
        public string? StopName { get; set; }
        public bool IsActive { get; set; }

        public string StatusLabel => IsActive ? "Active" : "Inactive";
        public Color StatusColor => IsActive ? Colors.Green : Colors.Red;
        public string BusDisplay => BusName != null ? $"{BusName} ({BusNumber})" : "No bus assigned";
        public string InitialsDisplay => FullName.Length >= 2 ? FullName[..2].ToUpper() : FullName.ToUpper();
        public Color StatusBgColor => IsActive ? Color.FromArgb("#d1fae5") : Color.FromArgb("#f1f5f9");
        public Color StatusTextColor => IsActive ? Color.FromArgb("#065f46") : Color.FromArgb("#475569");
    }
}
