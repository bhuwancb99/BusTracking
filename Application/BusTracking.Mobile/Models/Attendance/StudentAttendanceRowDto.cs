namespace BusTracking.Mobile.Models.Attendance
{
    public partial class StudentAttendanceRowDto : ObservableObject
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(StatusColor))]
        [NotifyPropertyChangedFor(nameof(StatusBgColor))]
        private string _status = "Present";

        public bool IsFaceScanned { get; set; }
        public double MatchConfidence { get; set; }

        public Color StatusColor => Status switch
        {
            "Present" => Color.FromArgb("#10b981"),
            "Absent" => Color.FromArgb("#ef4444"),
            "Late" => Color.FromArgb("#f59e0b"),
            _ => Color.FromArgb("#64748b")
        };

        public Color StatusBgColor => Status switch
        {
            "Present" => Color.FromArgb("#d1fae5"),
            "Absent" => Color.FromArgb("#fee2e2"),
            "Late" => Color.FromArgb("#fef3c7"),
            _ => Color.FromArgb("#f1f5f9")
        };
    }
}
