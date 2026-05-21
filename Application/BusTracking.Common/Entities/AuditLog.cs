namespace BusTracking.Common.Entities
{
    public class AuditLog
    {
        [Key] public long AuditLogId { get; set; }
        public int? UserId { get; set; }
        [Required, MaxLength(100)] public string Action { get; set; } = "";
        [MaxLength(100)] public string? EntityName { get; set; }
        [MaxLength(50)] public string? EntityId { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        [MaxLength(50)] public string? IpAddress { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
