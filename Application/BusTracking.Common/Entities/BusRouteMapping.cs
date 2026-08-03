namespace BusTracking.Common.Entities
{
    [Table("BusRouteMappings")]
    public class BusRouteMapping : IMultiTenant
    {
        public int? SchoolId { get; set; }
        public int BusId { get; set; }
        public int RouteId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(BusId))]
        public virtual Bus Bus { get; set; } = null!;

        [ForeignKey(nameof(RouteId))]
        public virtual BusRoute Route { get; set; } = null!;
    }
}
