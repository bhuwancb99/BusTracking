namespace BusTracking.Common.Entities
{
    public class Feedback
    {
        [Key] public int FeedbackId { get; set; }
        public int UserId { get; set; }
        public FeedbackCategory Category { get; set; }
        [Required, MaxLength(255)] public string Email { get; set; } = "";
        [MaxLength(20)] public string? PhoneNumber { get; set; }
        [Required, MaxLength(2000)] public string Description { get; set; } = "";
        public FeedbackStatus Status { get; set; } = FeedbackStatus.Open;
        public int? ResolvedBy { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(UserId))] public User User { get; set; } = null!;
    }
}
