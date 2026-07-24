namespace BusTracking.Mobile.Interfaces
{
    public interface ITripService
    {
        Task<PagedResult<TripItem>> GetAllAsync(string? status = null, string? date = null, int page = 1);
        Task<TripItem?> GetByIdAsync(int id);
        Task<ApiResponse<object>> CreateAsync(CreateTripRequest req);
        Task<ApiResponse<object>> StartAsync(int id);
        Task<ApiResponse<object>> EndAsync(int id);
        Task<ApiResponse<object>> CancelAsync(int id);
        Task<ApiResponse<object>> DeleteAsync(int id);
        Task<BusLocation?> GetLocationAsync(int tripId);
        Task<ApiResponse<List<StudentTripStatusDto>>> GetTripStudentsAsync(int tripId);
    }
}
