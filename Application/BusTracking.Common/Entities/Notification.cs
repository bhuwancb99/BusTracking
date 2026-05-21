namespace BusTracking.Common.Entities
{
    public class Notification
    {
        [Key] public int NotificationId { get; set; }
        public int RecipientUserId { get; set; }
        [Required, MaxLength(200)] public string Title { get; set; } = "";
        [Required, MaxLength(1000)] public string Body { get; set; } = "";
        public NotificationType NotificationType { get; set; }
        public int? ReferenceId { get; set; }
        [MaxLength(50)] public string? ReferenceType { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }

        [ForeignKey(nameof(RecipientUserId))] public User Recipient { get; set; } = null!;
    }
}
