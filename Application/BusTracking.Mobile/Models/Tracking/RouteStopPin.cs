namespace BusTracking.Mobile.Models.Tracking
{
    public class RouteStopPin
    {
        public string StopName { get; set; } = "";
        public int StopOrder { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Status { get; set; } = "Pending"; // Pending/Reached/Departed
    }
}
