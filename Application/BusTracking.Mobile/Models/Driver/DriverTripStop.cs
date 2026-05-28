namespace BusTracking.Mobile.Models.Driver
{
    public class DriverTripStop
    {
        public int StopId { get; set; }
        public string StopName { get; set; } = "";
        public int StopOrder { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string Status { get; set; } = "Pending"; // Pending | Reached | Departed
        public DateTime? ReachedAt { get; set; }
        public DateTime? DepartedAt { get; set; }
        public List<DriverStudentStatus> Students { get; set; } = [];

        public Color StatusColor => Status switch
        {
            "Reached" => Color.FromArgb("#1a73e8"),
            "Departed" => Color.FromArgb("#1e8e3e"),
            _ => Colors.Gray
        };
        public string StatusLabel => Status switch
        {
            "Reached" => "✅ Reached",
            "Departed" => "🚌 Departed",
            _ => "🕐 Pending"
        };
    }
}
