namespace BusTracking.Mobile.Models.Route
{
    public class StopItem
    {
        public int StopId { get; set; }
        public string StopName { get; set; } = "";
        public int StopOrder { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? MorningTime { get; set; }
        public string? EveningTime { get; set; }
    }
}
