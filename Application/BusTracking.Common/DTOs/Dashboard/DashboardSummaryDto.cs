namespace BusTracking.Common.DTOs.Dashboard
{
    public class DashboardSummaryDto
    {
        public int TotalBuses { get; set; }
        public int TotalDrivers { get; set; }
        public int TotalBusCoordinators { get; set; }
        public int TotalParents { get; set; }
        public int TotalStudents { get; set; }
        public int ActiveTrips { get; set; }
    }
}
