namespace BusTracking.Common.Entities
{
    public class CountryMaster
    {
        public int CountryId { get; set; }
        public string CountryName { get; set; } = string.Empty;
        public string? ISO2 { get; set; }
        public string? PhoneCode { get; set; }
        public string? CurrencyCode { get; set; }
        public string? CurrencySymbol { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<RegionMaster> Regions { get; set; } = new List<RegionMaster>();
        public virtual ICollection<School> Schools { get; set; } = new List<School>();
    }
}
