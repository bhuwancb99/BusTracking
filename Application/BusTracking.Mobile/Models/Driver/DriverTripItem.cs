namespace BusTracking.Mobile.Models.Driver
{
    public class DriverTripItem
    {
        public int TripId { get; set; }
        public string BusName { get; set; } = "";
        public string BusNumber { get; set; } = "";
        public string RouteName { get; set; } = "";
        public string TripType { get; set; } = "";
        public string TripDate { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }

        public Color StatusColor => Status switch
        {
            "InProgress" => Colors.Green,
            "Completed" => Colors.Gray,
            "Cancelled" => Colors.Red,
            _ => Colors.Orange
        };
        public string TripTypeIcon => TripType == "Morning" ? "🌅" : "🌆";
        public bool CanStart => Status == "Scheduled";
        public bool CanEnd => Status == "InProgress";
        public bool CanCancel => Status is "Scheduled" or "InProgress";
    }
}
