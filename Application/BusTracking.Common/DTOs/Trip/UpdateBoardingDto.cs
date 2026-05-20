namespace BusTracking.Common.DTOs.Trip
{
    public class UpdateBoardingDto
    {
        public int StudentId { get; set; }
        public int StopId { get; set; }
        public string BoardingStatus { get; set; } = "";  // PickedUp | NoShow | OnLeave
    }
}
