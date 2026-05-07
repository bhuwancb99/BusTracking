using BusTracking.Common.Enums;

namespace BusTracking.Common.DTOs.Availability
{
    public class CreateAvailabilityDto
    {
        public int StudentId { get; set; }
        public AvailabilityType AvailabilityType { get; set; }
        public string FromDate { get; set; } = "";   // "yyyy-MM-dd"
        public string ToDate { get; set; } = "";
        public string? Remarks { get; set; }
    }
}
