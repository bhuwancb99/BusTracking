namespace BusTracking.Common.DTOs.Bus
{
    public class UpdateBusDto
    {
        public string BusName { get; set; } = "";
        public string BusNumber { get; set; } = "";
        public List<int> RouteIds { get; set; } = [];

        [Required(ErrorMessage = "Bus type is required.")]
        public int BusTypeId { get; set; }

        public int? Capacity { get; set; }
        public List<int> DriverUserIds { get; set; } = [];
        public bool IsActive { get; set; } = true;

        // Compliance & Maintenance Tracking
        public DateOnly? InsuranceExpiryDate { get; set; }
        public DateOnly? FitnessExpiryDate { get; set; }
        public DateOnly? PucExpiryDate { get; set; }
        public DateOnly? LastServiceDate { get; set; }
    }
}
