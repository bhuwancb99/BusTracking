namespace BusTracking.Common.Entities
{
    public class BusFuelLog : IMultiTenant
    {
        public int? SchoolId { get; set; }

        [Key] public int FuelLogId { get; set; }
        public int BusId { get; set; }
        public int? DriverId { get; set; }
        public decimal OdometerReading { get; set; }
        public decimal FuelLiters { get; set; }
        public decimal TotalCost { get; set; }
        public DateOnly FuelDate { get; set; }
        [MaxLength(500)] public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(BusId))] public Bus Bus { get; set; } = null!;
        [ForeignKey(nameof(DriverId))] public User? Driver { get; set; }
    }
}
