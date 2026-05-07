namespace BusTracking.Common.DTOs.Bus
{
    public class BusDetailDto : BusListDto
    {
        public int? RouteId { get; set; }
        public string? RouteCode { get; set; }
        public int? DriverUserId { get; set; }
        public int StudentCount { get; set; }
    }
}
