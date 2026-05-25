namespace BusTracking.Common.DTOs.AppConfig
{
    public class UpdateAppConfigDto
    {
        [Required, MaxLength(100)] public string ConfigKey { get; set; } = "";
        [Required, MaxLength(500)] public string ConfigValue { get; set; } = "";
        [MaxLength(200)] public string? Description { get; set; }
        public ConfigPlatform Platform { get; set; } = ConfigPlatform.Both;
        public bool IsActive { get; set; } = true;
    }
}
