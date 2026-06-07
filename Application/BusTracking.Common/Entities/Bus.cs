namespace BusTracking.Common.Entities
{
    public class Bus
    {
        [Key] public int BusId { get; set; }
        [Required, MaxLength(100)] public string BusName { get; set; } = "";
        [Required, MaxLength(50)] public string BusNumber { get; set; } = "";
        public int? RouteId { get; set; }
        public int? Capacity { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public int? CreatedBy { get; set; }

        [ForeignKey(nameof(RouteId))]
        public BusRoute? Route { get; set; }

        public ICollection<StudentDetail> Students { get; set; } = [];
        public DriverDetail? Driver { get; set; }
        public ICollection<BusTrip> Trips { get; set; } = [];
        public ICollection<BusImage> Images { get; set; } = [];   // ← ADDED
    }
}
