namespace BusTracking.Mobile.Models.Driver
{
    public partial class DriverStudentStatus : ObservableObject
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = "";
        public string StudentName { get; set; } = "";
        public string? StopName { get; set; }
        public int StopOrder { get; set; }
        public bool IsUnavailable { get; set; }
        public string? AvailabilityType { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(BoardingColor))]
        [NotifyPropertyChangedFor(nameof(BoardingIcon))]
        private string _boardingStatus = "Pending";

        public static List<string> StatusOptions { get; } = new() { "Pending", "PickedUp", "NoShow", "OnLeave" };

        public Color BoardingColor => BoardingStatus switch
        {
            "PickedUp" => Color.FromArgb("#16a34a"),
            "NoShow" => Color.FromArgb("#dc2626"),
            "OnLeave" => Color.FromArgb("#ea580c"),
            _ => Color.FromArgb("#64748b")
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
