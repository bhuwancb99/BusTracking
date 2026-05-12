namespace BusTracking.Common.DTOs.Bus
{
    public class UpdateBusDto
    {
        public string BusName { get; set; } = "";
        public string BusNumber { get; set; } = "";
        public int? RouteId { get; set; }
        public int? Capacity { get; set; }
        public int? DriverUserId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
