using BusTracking.Common.DTOs.Stop;

namespace BusTracking.Common.DTOs.Route
{
    public class RouteDetailDto : RouteListDto
    {
        public string? Description { get; set; }
        public List<StopDto> Stops { get; set; } = [];
    }
}
