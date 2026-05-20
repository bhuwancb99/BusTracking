namespace BusTracking.Common.DTOs.Trip
{
    public class CreateTripDto
    {
        public int BusId { get; set; }
        public int DriverId { get; set; }
        public int RouteId { get; set; }
        public string TripType { get; set; } = "Morning";   // Morning | Evening
        public string TripDate { get; set; } = "";          // yyyy-MM-dd
    }

}
