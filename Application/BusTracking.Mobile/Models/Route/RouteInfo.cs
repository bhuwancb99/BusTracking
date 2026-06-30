namespace BusTracking.Mobile.Models.Route
{
    public class RouteInfo
    {
        public int RouteId { get; set; }
        public string RouteName { get; set; } = "";
        public string RouteCode { get; set; } = "";
        public string? MorningTime { get; set; }
        public string? EveningTime { get; set; }
        public string? Description { get; set; }
        public int StopCount { get; set; }
        public bool IsActive { get; set; }
    }
}
