namespace BusTracking.Common.DTOs.Trip
{
    public class TripListDto
    {
        public int TripId { get; set; }
        public string BusNumber { get; set; } = "";
        public string DriverName { get; set; } = "";
        public string RouteName { get; set; } = "";
        public string? StartStopName { get; set; }
        public string? EndStopName { get; set; }
        public string FromLocation => string.IsNullOrWhiteSpace(StartStopName) ? "—" : StartStopName;
        public string ToLocation => string.IsNullOrWhiteSpace(EndStopName) ? "—" : EndStopName;
        public string TripType { get; set; } = "";
        public string TripDate { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
    }
}
