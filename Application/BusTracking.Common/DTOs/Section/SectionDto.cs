namespace BusTracking.Common.DTOs.Section
{
    public class SectionDto
    {
        public int SectionId { get; set; }
        public int StandardId { get; set; }
        public string StandardName { get; set; } = string.Empty;
        public string SectionName { get; set; } = "A";
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
