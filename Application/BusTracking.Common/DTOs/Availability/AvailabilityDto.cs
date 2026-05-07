namespace BusTracking.Common.DTOs.Availability
{
    public class AvailabilityDto
    {
        public int AvailabilityId { get; set; }
        public string AvailabilityType { get; set; } = "";
        public string FromDate { get; set; } = "";
        public string ToDate { get; set; } = "";
        public string? Remarks { get; set; }
    }
}
