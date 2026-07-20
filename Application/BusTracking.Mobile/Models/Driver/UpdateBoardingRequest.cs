namespace BusTracking.Mobile.Models.Driver
{
    public class UpdateBoardingRequest
    {
        public int TripId { get; set; }
        public int StudentId { get; set; }
        public int StopId { get; set; }
        public string BoardingStatus { get; set; } = "";
        public string Status { get; set; } = "";
    }
}
