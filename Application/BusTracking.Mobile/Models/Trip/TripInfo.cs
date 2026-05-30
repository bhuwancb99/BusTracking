namespace BusTracking.Mobile.Models.Trip
{
    public class TripInfo
    {
        public int TripId { get; set; }
        public string? TripType { get; set; }
        public string? Status { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
    }
}
