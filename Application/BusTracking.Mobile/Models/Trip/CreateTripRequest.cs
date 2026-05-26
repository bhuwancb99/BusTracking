namespace BusTracking.Mobile.Models.Trip
{
    public class CreateTripRequest
    {
        public int BusId { get; set; }
        public int RouteId { get; set; }
        public string TripType { get; set; } = "Morning";
        public DateTime? TripDate { get; set; }
    }
}
