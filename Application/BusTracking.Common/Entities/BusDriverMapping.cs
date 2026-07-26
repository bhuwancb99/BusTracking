namespace BusTracking.Common.Entities
{
    public class BusDriverMapping : IMultiTenant
    {
        public int? SchoolId { get; set; }

        [Key] public int BusDriverMappingId { get; set; }
        public int BusId { get; set; }
        public int DriverUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(BusId))] public Bus Bus { get; set; } = null!;
        [ForeignKey(nameof(DriverUserId))] public User DriverUser { get; set; } = null!;
    }
}
