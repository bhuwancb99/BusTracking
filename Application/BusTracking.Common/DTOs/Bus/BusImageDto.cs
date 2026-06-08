namespace BusTracking.Common.DTOs.Bus
{
    public class BusImageDto
    {
        public int BusImageId { get; set; }
        public string ImageUrl { get; set; } = "";
        public int DisplayOrder { get; set; }
        public bool IsPrimary { get; set; }
    }
}
