namespace BusTracking.Common.DTOs.Availability
{
    public class UpdateAvailabilityDto
    {
        public int AvailabilityId { get; set; }
        public int StudentId { get; set; }
        public AvailabilityType AvailabilityType { get; set; }
        public string FromDate { get; set; } = "";
        public string ToDate { get; set; } = "";
        public string? Remarks { get; set; }
    }
}
