namespace BusTracking.Common.Models
{
    public class RouteStopViewModel
    {
        public int RouteId { get; set; }
        public string RouteName { get; set; } = "";
        public string RouteCode { get; set; } = "";
        public List<StopOptionViewModel> Stops { get; set; } = [];
    }
}
