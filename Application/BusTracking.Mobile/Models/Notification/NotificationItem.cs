namespace BusTracking.Mobile.Models.Notification
{
    public class NotificationItem
    {
        public int NotificationId { get; set; }
        public string Title { get; set; } = "";
        public string Body { get; set; } = "";
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }

        public string TimeDisplay => CreatedAt.ToLocalTime().ToString("dd MMM, HH:mm");
        public Color BgColor => IsRead ? Colors.White : Color.FromArgb("#f0f7ff");
    }
}
