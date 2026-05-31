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
        public Color StatusBgColor => Status switch
        {
            "InProgress" => Color.FromArgb("#d1fae5"),
            "Completed" => Color.FromArgb("#f1f5f9"),
            "Cancelled" => Color.FromArgb("#fee2e2"),
            _ => Color.FromArgb("#fef3c7"),
        };
        public Color StatusTextColor => Status switch
        {
            "InProgress" => Color.FromArgb("#065f46"),
            "Completed" => Color.FromArgb("#475569"),
            "Cancelled" => Color.FromArgb("#991b1b"),
            _ => Color.FromArgb("#92400e"),
        };
    }
}
