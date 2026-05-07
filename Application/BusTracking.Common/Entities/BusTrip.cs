using BusTracking.Common.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusTracking.Common.Entities
{
    public class BusTrip
    {
        [Key] public int TripId { get; set; }
        public int BusId { get; set; }
        public int DriverId { get; set; }
        public int RouteId { get; set; }
        public TripType TripType { get; set; }
        public DateOnly TripDate { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public TripStatus Status { get; set; } = TripStatus.Scheduled;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(BusId))] public Bus Bus { get; set; } = null!;
        [ForeignKey(nameof(DriverId))] public User Driver { get; set; } = null!;
        [ForeignKey(nameof(RouteId))] public BusRoute Route { get; set; } = null!;

        public ICollection<TripStopEvent> StopEvents { get; set; } = [];
        public ICollection<StudentTripStatus> StudentStatuses { get; set; } = [];
        public ICollection<BusLiveLocation> LiveLocations { get; set; } = [];
    }
}
