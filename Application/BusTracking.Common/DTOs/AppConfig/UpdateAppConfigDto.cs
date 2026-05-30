namespace BusTracking.Common.DTOs.AppConfig
{
    public class UpdateAppConfigDto
    {
        [Required, MaxLength(100)] public string ConfigKey { get; set; } = "";
        [Required, MaxLength(500)] public string ConfigValue { get; set; } = "";
        [MaxLength(200)] public string? Description { get; set; }

        public string Platform { get; set; } = "Both";

        [JsonIgnore]
        public ConfigPlatform PlatformEnum =>
            Enum.TryParse<ConfigPlatform>(Platform, ignoreCase: true, out var p)
                ? p
                : ConfigPlatform.Both;

        public bool IsActive { get; set; } = true;
    }
}