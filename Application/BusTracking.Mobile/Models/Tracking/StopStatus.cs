namespace BusTracking.Mobile.Models.Tracking
{
    public class StopStatus
    {
        public int StopId { get; set; }
        public string StopName { get; set; } = "";
        public int StopOrder { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime? ReachedAt { get; set; }
        public DateTime? DepartedAt { get; set; }

        public bool IsChildStop { get; set; }
        public string? ChildStudentName { get; set; }

        public Color StatusColor => IsChildStop
            ? Color.FromArgb("#7c3aed")
            : Status switch
            {
                "Reached" => Color.FromArgb("#1a73e8"),
                "Departed" => Color.FromArgb("#1e8e3e"),
                _ => Colors.Gray
            };
    }
}
