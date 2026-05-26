namespace BusTracking.Mobile.Models.Trip
{
    public class TripItem
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

        public Color StatusColor => Status switch
        {
            "InProgress" => Colors.Green,
            "Completed" => Colors.Gray,
            "Cancelled" => Colors.Red,
            _ => Colors.Orange
        };
        public string TripTypeIcon => TripType == "Morning" ? "🌅" : "🌆";
    }
}
