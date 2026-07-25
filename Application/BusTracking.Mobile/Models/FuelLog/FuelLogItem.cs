namespace BusTracking.Mobile.Models.FuelLog
{
    public class FuelLogItem
    {
        public int FuelLogId { get; set; }
        public int BusId { get; set; }
        public string BusNumber { get; set; } = string.Empty;
        public string BusName { get; set; } = string.Empty;
        public string DriverName { get; set; } = string.Empty;
        public decimal OdometerReading { get; set; }
        public decimal FuelLiters { get; set; }
        public decimal TotalCost { get; set; }
        public string FuelDate { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
