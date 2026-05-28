namespace BusTracking.Mobile.Models.Driver
{
    public class DriverDashboardData
    {
        public string BusName { get; set; } = "";
        public string BusNumber { get; set; } = "";
        public string RouteName { get; set; } = "";
        public int TotalStudents { get; set; }
        public DriverTripItem? ActiveTrip { get; set; }
    }
}
