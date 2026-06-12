namespace BusTracking.Common.DTOs.Bus
{
    public class CreateBusDto
    {
        public string BusName { get; set; } = "";
        public string BusNumber { get; set; } = "";
        public int? RouteId { get; set; }

        [Required(ErrorMessage = "Bus type is required.")]
        public int BusTypeId { get; set; }

        public int? Capacity { get; set; }
        public int? DriverUserId { get; set; }   // optional assign on create
    }
}
