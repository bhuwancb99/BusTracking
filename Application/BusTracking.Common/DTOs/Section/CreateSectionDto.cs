namespace BusTracking.Common.DTOs.Section
{
    public class CreateSectionDto
    {
        public int StandardId { get; set; }
        public string SectionName { get; set; } = "A";
        public bool IsDefault { get; set; } = false;
    }
}
