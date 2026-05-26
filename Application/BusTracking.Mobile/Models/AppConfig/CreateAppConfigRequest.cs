namespace BusTracking.Mobile.Models.AppConfig
{
    public class CreateAppConfigRequest
    {
        public string ConfigKey { get; set; } = "";
        public string ConfigValue { get; set; } = "";
        public string? Description { get; set; }
        public string Platform { get; set; } = "Both";
        public bool IsActive { get; set; } = true;
    }
}
