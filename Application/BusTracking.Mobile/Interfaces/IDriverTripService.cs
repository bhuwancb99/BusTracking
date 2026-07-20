namespace BusTracking.Mobile.Interfaces
{
    /// <summary>
    /// Service used exclusively by the Driver role for managing their own trips.
    /// Targets /api/driver/* endpoints.
    /// </summary>
    public interface IDriverTripService
    {
        /// <summary>
        /// Get today's trip(s) assigned to the logged-in driver.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        Task<List<DriverTripItem>> GetMyTripsAsync(string? date = null);

        /// <summary>
        /// Get all stops with student lists for a trip.
        /// </summary>
        /// <param name="tripId"></param>
        /// <returns></returns>
        Task<List<DriverTripStop>> GetTripStopsAsync(int tripId);

        /// <summary>
        /// Start a trip (sets Status → InProgress, records StartedAt).
        /// </summary>
        /// <param name="tripId"></param>
        /// <returns></returns>
        Task<ApiResponse<object>> StartTripAsync(int tripId);

        /// <summary>
        /// End a trip (sets Status → Completed, records EndedAt).
        /// </summary>
        /// <param name="tripId"></param>
        /// <returns></returns>
        Task<ApiResponse<object>> EndTripAsync(int tripId);

        /// <summary>
        /// Cancel a trip.
        /// </summary>
        /// <param name="tripId"></param>
        /// <returns></returns>
        Task<ApiResponse<object>> CancelTripAsync(int tripId);

        /// <summary>
        /// Update a single student's boarding status.
        /// </summary>
        /// <param name="tripId"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        Task<ApiResponse<object>> UpdateBoardingAsync(int tripId, UpdateBoardingRequest req);

        /// <summary>
        /// Post a GPS location ping while tracking is active.
        /// </summary>
        /// <param name="tripId"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        Task<ApiResponse<object>> PingLocationAsync(int tripId, LocationPingRequest req);

        /// <summary>
        /// Get the driver's own dashboard summary (bus/route/student count).
        /// </summary>
        /// <returns></returns>
        Task<DriverDashboardData?> GetDashboardAsync();

        /// <summary>
        /// Get all students assigned to a trip with their boarding status and stop info.
        /// </summary>
        Task<List<DriverStudentStatus>> GetTripStudentsAsync(int tripId);

        /// <summary>
        /// Mark a stop as reached.
        /// </summary>
        Task<ApiResponse<object>> ReachStopAsync(int tripId, int stopId);

        /// <summary>
        /// Mark a stop as departed.
        /// </summary>
        Task<ApiResponse<object>> DepartStopAsync(int tripId, int stopId);
    }
}