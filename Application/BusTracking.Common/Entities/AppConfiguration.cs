namespace BusTracking.Common.Entities
{
    public class AppConfiguration : IMultiTenant
    {
        public int? SchoolId { get; set; }

        [Key] public int ConfigId { get; set; }

        [Required, MaxLength(100)]
        public string ConfigKey { get; set; } = "";        // e.g. IsMaintencePage, Android_Update_Url

        [Required, MaxLength(500)]
        public string ConfigValue { get; set; } = "";      // e.g. 1, https://play.google.com/...

        [MaxLength(200)]
        public string? Description { get; set; }           // human-readable explanation

        public ConfigPlatform Platform { get; set; } = ConfigPlatform.Both;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public int CreatedBy { get; set; }

        [ForeignKey(nameof(CreatedBy))]
        public User CreatedByUser { get; set; } = null!;
    }
}
