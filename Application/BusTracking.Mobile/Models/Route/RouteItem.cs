namespace BusTracking.Mobile.Models.Route
{
    public class RouteItem
    {
        public int RouteId { get; set; }
        public string RouteName { get; set; } = "";
        public string RouteCode { get; set; } = "";
        public string? MorningTime { get; set; }
        public string? EveningTime { get; set; }
        public int StopCount { get; set; }
        public bool IsActive { get; set; }

        public string StatusLabel => IsActive ? "Active" : "Inactive";
        public string TimingDisplay => $"{MorningTime ?? "--"} / {EveningTime ?? "--"}";
    }
}
