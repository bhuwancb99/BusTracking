namespace BusTracking.Common.Entities
{
    [Table("BusFuelLogs")]
    public class BusFuelLog : IMultiTenant
    {
        public int? SchoolId { get; set; }

        [Key] public int FuelLogId { get; set; }
        public int BusId { get; set; }

        [Column("DriverId")]
        public int? DriverId { get; set; }

        [Column("OdometerReading")]
        public decimal OdometerReading { get; set; }

        [Column("FuelLiters")]
        public decimal FuelLiters { get; set; }

        public decimal TotalCost { get; set; }

        [Column("FuelDate")]
        public DateOnly FuelDate { get; set; }

        [MaxLength(500), Column("Notes")]
        public string? Notes { get; set; }

        [MaxLength(500)]
        public string? ReceiptImage { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(BusId))] public virtual Bus Bus { get; set; } = null!;
        [ForeignKey(nameof(DriverId))] public virtual User? Driver { get; set; }
    }
}
