namespace BusTracking.Common.DTOs.Trip
{
    public class UpdateBoardingRequest
    {
        public int TripId { get; set; }
        public int StudentId { get; set; }
        public int StopId { get; set; }
        public string Status { get; set; } = "";
    }

}
