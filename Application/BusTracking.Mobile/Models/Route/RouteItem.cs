namespace BusTracking.Mobile.Models.Route
{
    public class RouteItem
    {
        public int RouteId { get; set; }
        public string RouteName { get; set; } = "";
        public string RouteCode { get; set; } = "";
        public string? MorningTime { get; set; }
        public string? EveningTime { get; set; }
        public string? Description { get; set; }
        public int StopCount { get; set; }
        public bool IsActive { get; set; }

        public string StatusLabel => IsActive ? "Active" : "Inactive";
        public string TimingDisplay => $"{MorningTime ?? "--"} / {EveningTime ?? "--"}";
        public Color StatusBgColor => IsActive ? Color.FromArgb("#d1fae5") : Color.FromArgb("#f1f5f9");
        public Color StatusTextColor => IsActive ? Color.FromArgb("#065f46") : Color.FromArgb("#475569");
        public Color StatusColor => IsActive ? Colors.Green : Colors.Red;
    }
}
