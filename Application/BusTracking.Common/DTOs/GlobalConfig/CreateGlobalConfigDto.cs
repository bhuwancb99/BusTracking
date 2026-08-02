namespace BusTracking.Common.DTOs.GlobalConfig
{
    public class CreateGlobalConfigDto
    {
        [Required(ErrorMessage = "Key is required.")]
        [MaxLength(100, ErrorMessage = "Key cannot exceed 100 characters.")]
        public string GlobalConfigKey { get; set; } = "";

        [Required(ErrorMessage = "Value is required.")]
        [MaxLength(1000, ErrorMessage = "Value cannot exceed 1000 characters.")]
        public string GlobalConfigValue { get; set; } = "";

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
