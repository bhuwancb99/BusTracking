namespace BusTracking.Mobile.Models.Driver
{
    public class LocationPingRequest
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Speed { get; set; }
        public double? Heading { get; set; }
    }
}
