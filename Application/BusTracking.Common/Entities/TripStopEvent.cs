namespace BusTracking.Common.Entities
{
    public class TripStopEvent
    {
        [Key] public int TripStopEventId { get; set; }
        public int TripId { get; set; }
        public int StopId { get; set; }
        public DateTime? ReachedAt { get; set; }
        public DateTime? DepartedAt { get; set; }
        public TripStopStatus Status { get; set; } = TripStopStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(TripId))] public BusTrip Trip { get; set; } = null!;
        [ForeignKey(nameof(StopId))] public Stop Stop { get; set; } = null!;
    }
}
