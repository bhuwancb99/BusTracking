namespace BusTracking.Mobile.Models.Route
{
    public class CreateRouteRequest
    {
        public string RouteName { get; set; } = "";
        public string RouteCode { get; set; } = "";
        public string? MorningTime { get; set; }
        public string? EveningTime { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
