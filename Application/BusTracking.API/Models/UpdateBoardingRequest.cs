namespace BusTracking.API.Models
{
    public class UpdateBoardingRequestApi
    {
        public int StudentId { get; set; }
        public int StopId { get; set; }
        public string Status { get; set; } = "";
    }
}
