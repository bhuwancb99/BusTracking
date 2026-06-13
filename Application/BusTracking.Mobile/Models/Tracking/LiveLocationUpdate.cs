namespace BusTracking.Mobile.Models.Tracking
{
    public class LiveLocationUpdate
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public decimal? Speed { get; set; }
        public decimal? Heading { get; set; }
        public DateTime RecordedAt { get; set; }

        public string SpeedLabel =>
            Speed.HasValue ? $"{(int)Speed} km/h" : "";

        public string TimeLabel =>
            RecordedAt.ToLocalTime().ToString("HH:mm:ss");
    }
}
