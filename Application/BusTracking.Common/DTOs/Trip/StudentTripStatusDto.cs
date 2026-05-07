namespace BusTracking.Common.DTOs.Trip
{
    public class StudentTripStatusDto
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = "";
        public string StudentName { get; set; } = "";
        public string StopName { get; set; } = "";
        public int StopOrder { get; set; }
        public string BoardingStatus { get; set; } = "";
        public bool IsUnavailable { get; set; }
    }
}
