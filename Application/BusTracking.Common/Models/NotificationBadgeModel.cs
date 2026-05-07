namespace BusTracking.Common.Models
{
    public class NotificationBadgeModel
    {
        public int UnreadCount { get; set; }
        public bool HasUnread => UnreadCount > 0;
        public string BadgeText => UnreadCount > 99 ? "99+" : UnreadCount.ToString();
    }
}
