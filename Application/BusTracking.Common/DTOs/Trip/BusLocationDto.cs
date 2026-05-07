namespace BusTracking.Common.DTOs.Trip
{
    public class BusLocationDto
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public decimal? Speed { get; set; }
        public decimal? Heading { get; set; }
        public DateTime RecordedAt { get; set; }
    }
}
