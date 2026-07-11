namespace BusTracking.Common.DTOs.Stop
{
    public class ReorderStopsDto
    {
        public int RouteId { get; set; }
        public List<StopOrderItemDto> Stops { get; set; } = [];
    }
}
