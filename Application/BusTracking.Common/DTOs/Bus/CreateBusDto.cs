namespace BusTracking.Common.DTOs.Bus
{
    public class CreateBusDto
    {
        public string BusName { get; set; } = "";
        public string BusNumber { get; set; } = "";
        public int? RouteId { get; set; }
        public int? Capacity { get; set; }
    }
}
