namespace BusTracking.Common.Entities
{
    [Table("BusDriverMappings")]
    public class BusDriverMapping : IMultiTenant
    {
        public int? SchoolId { get; set; }
        public int BusId { get; set; }

        [Column("DriverId")]
        public int DriverUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(BusId))]
        public virtual Bus Bus { get; set; } = null!;

        [ForeignKey(nameof(DriverUserId))]
        public virtual User DriverUser { get; set; } = null!;
    }
}
