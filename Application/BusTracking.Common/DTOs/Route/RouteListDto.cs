namespace BusTracking.Common.DTOs.Route
{
    public class RouteListDto
    {
        public int RouteId { get; set; }
        public string RouteName { get; set; } = "";
        public string RouteCode { get; set; } = "";
        public string? MorningTime { get; set; }
        public string? EveningTime { get; set; }
        public int StopCount { get; set; }
        public bool IsActive { get; set; }
    }
}
