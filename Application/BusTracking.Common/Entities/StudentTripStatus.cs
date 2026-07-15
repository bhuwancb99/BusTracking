namespace BusTracking.Common.Entities
{
    public class StudentTripStatus : IMultiTenant
    {
        public int? SchoolId { get; set; }

        [Key] public int StudentTripStatusId { get; set; }
        public int TripId { get; set; }
        public int StudentId { get; set; }
        public int StopId { get; set; }
        public BoardingStatus BoardingStatus { get; set; } = BoardingStatus.Pending;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public int? UpdatedBy { get; set; }

        [ForeignKey(nameof(TripId))] public BusTrip Trip { get; set; } = null!;
        [ForeignKey(nameof(StudentId))] public StudentDetail Student { get; set; } = null!;
        [ForeignKey(nameof(StopId))] public Stop Stop { get; set; } = null!;
    }
}
