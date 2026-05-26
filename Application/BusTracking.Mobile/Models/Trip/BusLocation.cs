namespace BusTracking.Mobile.Models.Trip
{
    public class BusLocation
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public decimal? Speed { get; set; }
        public decimal? Heading { get; set; }
        public DateTime RecordedAt { get; set; }

        public string SpeedDisplay => $"{Speed?.ToString("F0") ?? "0"} km/h";
        public bool IsMoving => (Speed ?? 0) > 2;
    }
}
