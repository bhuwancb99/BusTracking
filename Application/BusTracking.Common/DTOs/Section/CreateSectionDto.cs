namespace BusTracking.Common.DTOs.Section
{
    public class CreateSectionDto
    {
        public int StandardId { get; set; }
        public string SectionName { get; set; } = "A";
        public int? ClassTeacherId { get; set; }
        public bool IsDefault { get; set; } = false;
    }
}
