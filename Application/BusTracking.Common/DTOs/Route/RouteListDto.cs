namespace BusTracking.Common.DTOs.Route
{
    public class RouteListDto
    {
        public int RouteId { get; set; }
        public string RouteName { get; set; } = "";
        public string RouteCode { get; set; } = "";
        public string? StartStopName { get; set; }
        public string? EndStopName { get; set; }
        public string FromLocation => string.IsNullOrWhiteSpace(StartStopName) ? "—" : StartStopName;
        public string ToLocation => string.IsNullOrWhiteSpace(EndStopName) ? "—" : EndStopName;
        public string? MorningTime { get; set; }
        public string? EveningTime { get; set; }
        public int StopCount { get; set; }
        public bool IsActive { get; set; }
    }
}
