namespace BusTracking.Common.Entities
{
    public class RegionMaster
    {
        public int RegionId { get; set; }
        public int CountryId { get; set; }
        public string RegionName { get; set; } = string.Empty;
        public string? RegionCode { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual CountryMaster? Country { get; set; }
        public virtual ICollection<School> Schools { get; set; } = new List<School>();
    }
}
