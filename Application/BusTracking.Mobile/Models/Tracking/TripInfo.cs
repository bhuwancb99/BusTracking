namespace BusTracking.Mobile.Models.Tracking
{
    public class TripInfo
    {
        public int TripId { get; set; }
        public string TripType { get; set; } = "";
        public string Status { get; set; } = "";
        public string? DriverName { get; set; }
    }
}
