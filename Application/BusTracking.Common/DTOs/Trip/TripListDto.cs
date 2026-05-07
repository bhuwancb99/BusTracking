namespace BusTracking.Common.DTOs.Trip
{
    public class TripListDto
    {
        public int TripId { get; set; }
        public string BusNumber { get; set; } = "";
        public string DriverName { get; set; } = "";
        public string RouteName { get; set; } = "";
        public string TripType { get; set; } = "";
        public string TripDate { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
    }
}
