namespace BusTracking.Mobile.Models.Route
{
    public class UpdateStopsRequest
    {
        public int RouteId { get; set; }
        public List<UpdateStopItemRequest> Stops { get; set; } = [];
    }

    public class UpdateStopItemRequest
    {
        public int StopId { get; set; }
        public string StopName { get; set; } = "";
        public int StopOrder { get; set; }
        public string? MorningTime { get; set; }
        public string? EveningTime { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }
}
