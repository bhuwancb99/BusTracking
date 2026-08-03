namespace BusTracking.Common.DTOs.Region
{
    public class UpdateRegionDto
    {
        [Required, MaxLength(150)]
        public string RegionName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? RegionCode { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
