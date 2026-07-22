namespace BusTracking.Mobile.Models.Driver
{
    public class LocationPingRequest
    {
        public int TripId { get; set; }
        public int BusId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Speed { get; set; }
        public double? Heading { get; set; }
    }
}
