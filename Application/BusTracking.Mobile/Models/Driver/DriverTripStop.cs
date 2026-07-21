namespace BusTracking.Mobile.Models.Driver
{
    public partial class DriverTripStop : ObservableObject
    {
        public int StopId { get; set; }
        public string StopName { get; set; } = "";
        public int StopOrder { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(StatusColor))]
        [NotifyPropertyChangedFor(nameof(StatusLabel))]
        private string _status = "Pending"; // Pending | Reached | Departed

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

        // ── Convenience helpers ──────────────────────────────────────────────
        public string PickupTime => ReachedAt?.ToString("HH:mm") ?? "--:--";

        public string StudentName => Students.FirstOrDefault()?.StudentName ?? "—";

        public string StudentCode => Students.FirstOrDefault()?.StudentCode ?? "";

        public bool IsBoarded => Students.FirstOrDefault()?.BoardingStatus == "PickedUp";

        public string BoardingLabel => Students.FirstOrDefault()?.BoardingStatus switch
        {
            "PickedUp" => "Boarded ✓",
            "NoShow" => "No Show",
            "OnLeave" => "On Leave",
            _ => "Pending"
        };
    }
}
