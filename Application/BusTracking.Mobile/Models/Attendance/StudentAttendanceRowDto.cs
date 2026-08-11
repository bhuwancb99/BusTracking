namespace BusTracking.Mobile.Models.Attendance
{
    public partial class StudentAttendanceRowDto : ObservableObject
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }

        public bool HasProfileImage => !string.IsNullOrWhiteSpace(ProfileImageUrl);
        public string DisplayAvatarUrl => HasProfileImage ? ProfileImageUrl! : "avatar_placeholder.png";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(StatusColor))]
        [NotifyPropertyChangedFor(nameof(StatusBgColor))]
        private string _status = "Present";

        public bool IsFaceScanned { get; set; }
        public double MatchConfidence { get; set; }

        public Color StatusColor => Status switch
        {
            "Present" => Color.FromArgb("#059669"),
            "Absent" => Color.FromArgb("#dc2626"),
            _ => Color.FromArgb("#64748b")
        };

        public Color StatusBgColor => Status switch
        {
            "Present" => Color.FromArgb("#d1fae5"),
            "Absent" => Color.FromArgb("#fee2e2"),
            _ => Color.FromArgb("#f1f5f9")
        };
    }
}
