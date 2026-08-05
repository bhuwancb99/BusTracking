namespace BusTracking.Mobile.Models.Subject
{
    public class SubjectItem
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string? SubjectCode { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string DisplayName => string.IsNullOrWhiteSpace(SubjectCode) ? SubjectName : $"{SubjectName} ({SubjectCode})";
        public string StatusLabel => IsActive ? "Active" : "Inactive";
        public Color StatusBgColor => IsActive ? Color.FromArgb("#d1fae5") : Color.FromArgb("#f1f5f9");
        public Color StatusTextColor => IsActive ? Color.FromArgb("#065f46") : Color.FromArgb("#475569");
    }
}
