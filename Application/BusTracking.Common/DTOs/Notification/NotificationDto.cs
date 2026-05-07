namespace BusTracking.Common.DTOs.Notification
{
    public class NotificationDto
    {
        public int NotificationId { get; set; }
        public string Title { get; set; } = "";
        public string Body { get; set; } = "";
        public string NotificationType { get; set; } = "";
        public bool IsRead { get; set; }
        public DateTime SentAt { get; set; }
    }
}
