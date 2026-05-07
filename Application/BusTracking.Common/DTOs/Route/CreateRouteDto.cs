namespace BusTracking.Common.DTOs.Route
{
    public class CreateRouteDto
    {
        public string RouteName { get; set; } = "";
        public string RouteCode { get; set; } = "";
        public string? MorningTime { get; set; }   // "HH:mm"
        public string? EveningTime { get; set; }
        public string? Description { get; set; }
    }
}
