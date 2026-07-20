namespace BusTracking.Common.DTOs.Driver
{
    public class DriverTripSummary
    {
        public int TripId { get; set; }
        public string TripType { get; set; } = "";   // "Morning" | "Evening"
        public string TripDate { get; set; } = "";   // "yyyy-MM-dd"
        public string Status { get; set; } = "";   // "Scheduled" | "InProgress" | "Completed"
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }

        // Convenience flags for view logic
        public bool CanStart => Status == "Scheduled";
        public bool CanEnd => Status == "InProgress";
        public bool IsActive => Status == "InProgress";
    }
}
