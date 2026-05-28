namespace BusTracking.Mobile.Interfaces
{
    public interface ITripService
    {
        Task<List<TripItem>> GetAllAsync(string? status = null, string? date = null);
        Task<ApiResponse<object>> CreateAsync(CreateTripRequest req);
        Task<ApiResponse<object>> StartAsync(int id);
        Task<ApiResponse<object>> EndAsync(int id);
        Task<ApiResponse<object>> CancelAsync(int id);
        Task<BusLocation?> GetLocationAsync(int tripId);
    }
}
