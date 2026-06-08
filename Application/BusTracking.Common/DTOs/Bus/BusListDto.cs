namespace BusTracking.Common.DTOs.Bus
{
    public class BusListDto
    {
        public int BusId { get; set; }
        public string BusName { get; set; } = "";
        public string BusNumber { get; set; } = "";
        public string? RouteName { get; set; }
        public int? RouteId { get; set; }
        public int? DriverUserId { get; set; }
        public string? DriverName { get; set; }
        public string? DriverPhone { get; set; }
        public int? Capacity { get; set; }
        public int StudentCount { get; set; }
        public bool IsActive { get; set; }
        public string? PrimaryImageUrl { get; set; }
        public List<BusImageDto> Images { get; set; } = [];
    }
}
