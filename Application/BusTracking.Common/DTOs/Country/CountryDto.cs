namespace BusTracking.Common.DTOs.Country
{
    public class CountryDto
    {
        public int CountryId { get; set; }
        public string CountryName { get; set; } = string.Empty;
        public string? ISO2 { get; set; }
        public string? PhoneCode { get; set; }
        public string? CurrencyCode { get; set; }
        public string? CurrencySymbol { get; set; }
        public bool IsActive { get; set; }
        public int ActiveRegionCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
