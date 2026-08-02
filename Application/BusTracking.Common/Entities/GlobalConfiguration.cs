namespace BusTracking.Common.Entities
{
    public class GlobalConfiguration
    {
        [Key]
        public int GlobalConfigId { get; set; }

        [Required, MaxLength(100)]
        public string GlobalConfigKey { get; set; } = "";

        [Required, MaxLength(1000)]
        public string GlobalConfigValue { get; set; } = "";

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
