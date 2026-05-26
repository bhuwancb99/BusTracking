using BusTracking.Mobile.Models.Trip;

namespace BusTracking.Mobile.Models.Tracking
{
    public class TrackingData
    {
        public bool IsLive { get; set; }
        public string? Message { get; set; }
        public BusInfo? Bus { get; set; }
        public TripInfo? Trip { get; set; }
        public BusLocation? Location { get; set; }
        public string BoardingStatus { get; set; } = "Pending";
        public List<StopStatus> Stops { get; set; } = [];
    }
}
