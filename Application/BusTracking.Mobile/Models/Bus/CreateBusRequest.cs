namespace BusTracking.Mobile.Models.Bus
{
    public class CreateBusRequest
    {
        public string BusName { get; set; } = "";
        public string BusNumber { get; set; } = "";
        public int BusTypeId { get; set; }
        public int? RouteId { get; set; }
        public List<int> RouteIds { get; set; } = [];
        public int? Capacity { get; set; }
        public int? DriverUserId { get; set; }
        public List<int> DriverUserIds { get; set; } = [];
        public bool IsActive { get; set; } = true;

        // Compliance & Maintenance Tracking
        public string? InsuranceExpiryDate { get; set; }
        public string? FitnessExpiryDate { get; set; }
        public string? PucExpiryDate { get; set; }
        public string? LastServiceDate { get; set; }
    }
}
