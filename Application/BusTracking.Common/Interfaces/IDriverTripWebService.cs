namespace BusTracking.Common.Interfaces
{
    public interface IDriverTripWebService
    {
        Task<ApiResponse<DriverMyTripDto>> GetMyTripAsync(int driverUserId);
        Task<ApiResponse<bool>> StartTripAsync(int tripId, int driverUserId);
        Task<ApiResponse<bool>> EndTripAsync(int tripId, int driverUserId);
        Task<ApiResponse<List<StudentTripStatusDto>>> GetTripStudentsAsync(int tripId);
        Task<ApiResponse<List<TripStopEventDto>>> GetTripStopsAsync(int tripId);
        Task<ApiResponse<bool>> ReachStopAsync(int tripId, int stopId);
        Task<ApiResponse<bool>> DepartStopAsync(int tripId, int stopId);
        Task<ApiResponse<bool>> UpdateBoardingAsync(int tripId, int studentId, int stopId, string status);
        Task<ApiResponse<BusLocationDto?>> GetLatestLocationAsync(int tripId);
        Task<ApiResponse<List<BusLocationDto>>> GetLocationHistoryAsync(int tripId);
        Task<ApiResponse<bool>> InsertLocationPingAsync(int tripId, int busId, decimal lat, decimal lng, decimal? speed, decimal? heading);
    }
}
