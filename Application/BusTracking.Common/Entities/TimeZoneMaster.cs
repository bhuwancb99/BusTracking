namespace BusTracking.Common.Entities
{
    public class TimeZoneMaster
    {
        [Key]
        public int TimeZoneId { get; set; }

        [Required, MaxLength(200)]
        public string TimeZoneName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string IanaTimeZoneId { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string WindowsTimeZoneId { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string UtcOffset { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; }
    }
}
