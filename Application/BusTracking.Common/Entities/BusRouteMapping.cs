namespace BusTracking.Common.Entities
{
    public class BusRouteMapping : IMultiTenant
    {
        public int? SchoolId { get; set; }

        [Key] public int BusRouteMappingId { get; set; }
        public int BusId { get; set; }
        public int RouteId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(BusId))] public Bus Bus { get; set; } = null!;
        [ForeignKey(nameof(RouteId))] public BusRoute Route { get; set; } = null!;
    }
}
