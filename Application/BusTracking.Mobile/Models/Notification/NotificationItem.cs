namespace BusTracking.Mobile.Models.Notification
{
    public partial class NotificationItem : ObservableObject
    {
        public int NotificationId { get; set; }
        public string Title { get; set; } = "";
        public string Body { get; set; } = "";
        public string NotificationType { get; set; } = "";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsUnread))]
        [NotifyPropertyChangedFor(nameof(BgColor))]
        private bool _isRead;

        public DateTime SentAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public bool IsUnread => !IsRead;

        public string SentAtDisplay => (SentAt != default ? SentAt : CreatedAt).ToLocalTime().ToString("dd MMM yyyy, hh:mm tt");
        public string TimeDisplay => (SentAt != default ? SentAt : CreatedAt).ToLocalTime().ToString("dd MMM, HH:mm");
        public Color BgColor => IsRead ? Colors.White : Color.FromArgb("#f0f7ff");
    }
}
