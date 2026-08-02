namespace BusTracking.Common.DTOs.GlobalConfig
{
    public class GlobalConfigDto
    {
        public int GlobalConfigId { get; set; }
        public string GlobalConfigKey { get; set; } = "";
        public string GlobalConfigValue { get; set; } = "";
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
