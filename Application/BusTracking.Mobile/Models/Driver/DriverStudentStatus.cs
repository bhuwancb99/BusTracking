namespace BusTracking.Mobile.Models.Driver
{
    public partial class DriverStudentStatus : ObservableObject
    {
        public static Action<DriverStudentStatus>? StatusChangedCallback { get; set; }

        public int StudentId { get; set; }
        public int StopId { get; set; }
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

        partial void OnBoardingStatusChanged(string? oldValue, string newValue)
        {
            if (!string.IsNullOrEmpty(oldValue) && oldValue != newValue)
            {
                StatusChangedCallback?.Invoke(this);
            }
        }

        public void NotifyStatusChanged()
        {
            OnPropertyChanged(nameof(BoardingStatus));
            OnPropertyChanged(nameof(BoardingColor));
            OnPropertyChanged(nameof(BoardingIcon));
        }

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
            "PickedUp" => "picked_up.png",
            "NoShow" => "no_show.png",
            "OnLeave" => "on_leave.png",
            _ => "pending.png"
        };
    }
}
