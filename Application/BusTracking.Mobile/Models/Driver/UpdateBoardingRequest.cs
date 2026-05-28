namespace BusTracking.Mobile.Models.Driver
{
    public class UpdateBoardingRequest
    {
        public int StudentId { get; set; }
        public int StopId { get; set; }
        public string BoardingStatus { get; set; } = "";
    }
}
