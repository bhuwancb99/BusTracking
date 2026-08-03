namespace BusTracking.Common.DTOs.Country
{
    public class CreateCountryDto
    {
        [Required, MaxLength(150)]
        public string CountryName { get; set; } = string.Empty;

        [MaxLength(10)]
        public string? ISO2 { get; set; }

        [MaxLength(10)]
        public string? PhoneCode { get; set; }

        [MaxLength(10)]
        public string? CurrencyCode { get; set; }

        [MaxLength(10)]
        public string? CurrencySymbol { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
