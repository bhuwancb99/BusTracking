namespace BusTracking.Mobile.Models.AppConfig
{
    public class AppConfigItem
    {
        public int ConfigId { get; set; }
        public string ConfigKey { get; set; } = "";
        public string ConfigValue { get; set; } = "";
        public string? Description { get; set; }
        public string Platform { get; set; } = "";
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatedByName { get; set; } = "";

        public string StatusLabel => IsActive ? "Active" : "Inactive";
        public Color StatusColor => IsActive ? Colors.Green : Colors.Red;
        public Color PlatformColor => Platform switch
        {
            "Mobile" => Color.FromArgb("#1a73e8"),
            "Web" => Color.FromArgb("#1e8e3e"),
            _ => Colors.Gray
        };
        public Color StatusBgColor => IsActive ? Color.FromArgb("#d1fae5") : Color.FromArgb("#f1f5f9");
        public Color StatusTextColor => IsActive ? Color.FromArgb("#065f46") : Color.FromArgb("#475569");
    }
}
