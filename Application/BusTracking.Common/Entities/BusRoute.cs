namespace BusTracking.Common.Entities
{
    public class BusRoute : IMultiTenant
    {
        public int? SchoolId { get; set; }

        [Key] public int RouteId { get; set; }
        [Required, MaxLength(150)] public string RouteName { get; set; } = "";
        [Required, MaxLength(50)] public string RouteCode { get; set; } = "";
        public TimeOnly? MorningTime { get; set; }
        public TimeOnly? EveningTime { get; set; }
        [MaxLength(500)] public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public int? CreatedBy { get; set; }

        public ICollection<Stop> Stops { get; set; } = [];
        public ICollection<Bus> Buses { get; set; } = [];
    }
}
