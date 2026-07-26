namespace BusTracking.Common.Entities
{
    public class Bus : IMultiTenant
    {
        public int? SchoolId { get; set; }

        [Key] public int BusId { get; set; }
        [Required, MaxLength(100)] public string BusName { get; set; } = "";
        [Required, MaxLength(50)] public string BusNumber { get; set; } = "";
        [Required]
        public int BusTypeId { get; set; }

        public int? Capacity { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public int? CreatedBy { get; set; }

        // Compliance & Maintenance Tracking
        public DateOnly? InsuranceExpiryDate { get; set; }
        public DateOnly? FitnessExpiryDate { get; set; }
        public DateOnly? PucExpiryDate { get; set; }
        public DateOnly? LastServiceDate { get; set; }

        [ForeignKey(nameof(BusTypeId))]
        public BusTypeMaster? BusType { get; set; }

        public ICollection<StudentDetail> Students { get; set; } = [];
        public ICollection<BusTrip> Trips { get; set; } = [];
        public ICollection<BusImage> Images { get; set; } = [];
        public ICollection<BusFuelLog> FuelLogs { get; set; } = [];
        public ICollection<BusRouteMapping> RouteMappings { get; set; } = [];
        public ICollection<BusDriverMapping> DriverMappings { get; set; } = [];
    }
}
