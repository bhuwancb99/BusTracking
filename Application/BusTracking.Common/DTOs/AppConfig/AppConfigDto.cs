namespace BusTracking.Common.DTOs.AppConfig
{
    public class AppConfigDto
    {
        public int ConfigId { get; set; }
        public string ConfigKey { get; set; } = "";
        public string ConfigValue { get; set; } = "";
        public string? Description { get; set; }
        public string Platform { get; set; } = "";
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatedByName { get; set; } = "";
    }
}
