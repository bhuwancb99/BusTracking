namespace BusTracking.Mobile.Models.Route
{
    public class ReorderStopsRequest
    {
        public int RouteId { get; set; }
        public List<StopOrderItemRequest> Stops { get; set; } = [];
    }
}
