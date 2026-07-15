namespace BusTracking.Common.Entities
{
    public class NotificationSetting : IMultiTenant
    {
        public int? SchoolId { get; set; }

        [Key] public int NotificationSettingId { get; set; }
        [Required, MaxLength(50)] public string NotificationType { get; set; } = "";
        public bool IsEnabled { get; set; } = true;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public int? UpdatedBy { get; set; }
    }
}
