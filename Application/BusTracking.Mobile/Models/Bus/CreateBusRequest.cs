namespace BusTracking.Mobile.Models.Bus
{
    public class CreateBusRequest
    {
        public string BusName { get; set; } = "";
        public string BusNumber { get; set; } = "";
        public int BusTypeId { get; set; }
        public int? RouteId { get; set; }
        public int? Capacity { get; set; }
        public int? DriverUserId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
