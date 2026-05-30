namespace BusTracking.Common.DTOs.AppConfig
{
    public class CreateAppConfigDto
    {
        [Required, MaxLength(100)] public string ConfigKey { get; set; } = "";
        [Required, MaxLength(500)] public string ConfigValue { get; set; } = "";
        [MaxLength(200)] public string? Description { get; set; }

        // Accepts "Mobile", "Web", or "Both" (string) from the mobile app —
        // same pattern as CreateTripDto.TripType. Parsed to the enum by PlatformEnum.
        public string Platform { get; set; } = "Both";

        // Parsed value used internally by AppConfigService instead of Platform directly.
        [JsonIgnore]
        public ConfigPlatform PlatformEnum =>
            Enum.TryParse<ConfigPlatform>(Platform, ignoreCase: true, out var p)
                ? p
                : ConfigPlatform.Both;

        public bool IsActive { get; set; } = true;
    }
}