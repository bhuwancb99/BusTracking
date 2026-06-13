namespace BusTracking.Mobile.Models.Tracking
{
    public class TripRouteInfo
    {
        public int TripId { get; set; }
        public string BusName { get; set; } = "";
        public string BusNumber { get; set; } = "";
        public string RouteName { get; set; } = "";
        public string DriverName { get; set; } = "";
        public string TripStatus { get; set; } = "";
        public List<RouteStopPin> Stops { get; set; } = [];
    }
}
