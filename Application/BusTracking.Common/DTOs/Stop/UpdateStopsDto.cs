namespace BusTracking.Common.DTOs.Stop
{
    public class UpdateStopsDto
    {
        public int RouteId { get; set; }
        public List<StopDto> Stops { get; set; } = [];
    }
}
