namespace BusTracking.Common.DTOs.Trip
{
    public class TripStopEventDto
    {
        public int TripStopEventId { get; set; }
        public int StopId { get; set; }
        public string StopName { get; set; } = "";
        public int StopOrder { get; set; }
        public string Status { get; set; } = "";   // Pending | Reached | Departed
        public DateTime? ReachedAt { get; set; }
        public DateTime? DepartedAt { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

}
