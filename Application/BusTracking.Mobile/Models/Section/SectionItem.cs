namespace BusTracking.Mobile.Models.Section
{
    public class SectionItem
    {
        public int SectionId { get; set; }
        public int StandardId { get; set; }
        public string StandardName { get; set; } = string.Empty;
        public string SectionName { get; set; } = string.Empty;
        public int? ClassTeacherId { get; set; }
        public string? ClassTeacherName { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string StatusLabel => IsActive ? "Active" : "Inactive";
        public Color StatusBgColor => IsActive ? Color.FromArgb("#d1fae5") : Color.FromArgb("#f1f5f9");
        public Color StatusTextColor => IsActive ? Color.FromArgb("#065f46") : Color.FromArgb("#475569");
        public string TeacherDisplayName => !string.IsNullOrWhiteSpace(ClassTeacherName)
            ? $"👤 Class Teacher: {ClassTeacherName}"
            : "👤 Class Teacher: Not Assigned";
    }
}
