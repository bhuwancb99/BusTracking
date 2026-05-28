namespace BusTracking.Mobile.Models.Driver
{
    public class DriverStudentStatus
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = "";
        public string StudentName { get; set; } = "";
        public string BoardingStatus { get; set; } = "Pending"; // Pending | PickedUp | NoShow | OnLeave
        public bool IsUnavailable { get; set; }
        public string? AvailabilityType { get; set; }

        public Color BoardingColor => BoardingStatus switch
        {
            "PickedUp" => Color.FromArgb("#1e8e3e"),
            "NoShow" => Colors.Red,
            "OnLeave" => Colors.Orange,
            _ => Colors.Gray
        };
        public string BoardingIcon => BoardingStatus switch
        {
            "PickedUp" => "✅",
            "NoShow" => "❌",
            "OnLeave" => "📅",
            _ => "🔵"
        };
    }
}
