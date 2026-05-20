namespace BusTracking.Common.DTOs.Trip
{
    public class LocationPingDto
    {
        public int TripId { get; set; }
        public int BusId { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public decimal? Speed { get; set; }
        public decimal? Heading { get; set; }
    }
}
