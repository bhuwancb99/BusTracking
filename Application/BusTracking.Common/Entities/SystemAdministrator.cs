namespace BusTracking.Common.Entities
{
    public class SystemAdministrator
    {
        [Key]
        public int AdminId { get; set; }

        [Required, MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string UserName { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Email { get; set; }

        [Required, MaxLength(512)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required, MaxLength(256)]
        public string PasswordSalt { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
