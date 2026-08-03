namespace BusTracking.Common.DTOs.Region
{
    public class RegionDto
    {
        public int RegionId { get; set; }
        public int CountryId { get; set; }
        public string CountryName { get; set; } = string.Empty;
        public string RegionName { get; set; } = string.Empty;
        public string? RegionCode { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
