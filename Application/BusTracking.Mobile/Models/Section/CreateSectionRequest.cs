namespace BusTracking.Mobile.Models.Section
{
    public class CreateSectionRequest
    {
        public int StandardId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public int? ClassTeacherId { get; set; }
    }
}
