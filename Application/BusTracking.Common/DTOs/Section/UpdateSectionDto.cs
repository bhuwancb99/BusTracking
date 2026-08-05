namespace BusTracking.Common.DTOs.Section
{
    public class UpdateSectionDto
    {
        public int SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
