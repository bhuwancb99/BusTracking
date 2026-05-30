namespace BusTracking.Mobile.Models.Trip
{
    public class MyTripResponse
    {
        public BusTripInfo? Bus { get; set; }
        public RouteInfo? Route { get; set; }
        public TripInfo? Trip { get; set; }
    }
}
