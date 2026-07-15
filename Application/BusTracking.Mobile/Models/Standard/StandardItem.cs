namespace BusTracking.Mobile.Models.Standard
{
    public class StandardItem
    {
        public int StandardId { get; set; }
        public string StandardName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public string StatusLabel => IsActive ? "Active" : "Inactive";
        public Color StatusBgColor => IsActive ? Color.FromArgb("#d1fae5") : Color.FromArgb("#f1f5f9");
        public Color StatusTextColor => IsActive ? Color.FromArgb("#065f46") : Color.FromArgb("#475569");
    }
}
