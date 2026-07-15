namespace BusTracking.Common.Entities
{
    public class Logger
    {
        [Key]
        public int LogId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Platform { get; set; } = "";

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string? ExceptionMessage { get; set; }

        public string? StackTrace { get; set; }

        [MaxLength(2083)]
        public string? RequestUrl { get; set; }

        public int? UserId { get; set; }

        [MaxLength(256)]
        public string? Username { get; set; }

        [MaxLength(50)]
        public string? Role { get; set; }

        [MaxLength(100)]
        public string? ModuleName { get; set; }

        [MaxLength(100)]
        public string? ActionName { get; set; }

        public string? AdditionalDetails { get; set; }
    }
}
