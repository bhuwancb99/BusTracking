namespace BusTracking.Mobile.Models.Student
{
    public class StudentTripStatusDto
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = "";
        public string StudentName { get; set; } = "";
        public int StopId { get; set; }
        public string StopName { get; set; } = "";
        public int StopOrder { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string BoardingStatus { get; set; } = "";
        public bool IsUnavailable { get; set; }
    }
}
