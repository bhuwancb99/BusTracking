using BusTracking.Common.DTOs.Common;
using BusTracking.Common.DTOs.Trip;

namespace BusTracking.Common.Interfaces
{
    public interface ITripService
    {
        Task<ApiResponse<PagedResult<TripListDto>>> GetAllAsync(int page, int pageSize, string? busId);
        Task<ApiResponse<TripListDto>> GetByIdAsync(int tripId);
        Task<ApiResponse<TripListDto>> CreateAsync(CreateTripDto dto, int createdBy);
        Task<ApiResponse<bool>> StartTripAsync(int tripId);
        Task<ApiResponse<bool>> EndTripAsync(int tripId);
        Task<ApiResponse<bool>> CancelTripAsync(int tripId);
        Task<ApiResponse<bool>> DeleteAsync(int tripId);
        Task<ApiResponse<List<StudentTripStatusDto>>> GetTripStudentsAsync(int tripId);
        Task<ApiResponse<bool>> UpdateBoardingAsync(int tripId, int studentId, int stopId, string status);
        Task<ApiResponse<bool>> ReachStopAsync(int tripId, int stopId);
        Task<ApiResponse<List<TripStopEventDto>>> GetStopEventsAsync(int tripId);
        Task<ApiResponse<BusLocationDto?>> GetLatestLocationAsync(int tripId);
        Task<ApiResponse<bool>> InsertLocationPingAsync(int tripId, int busId, decimal lat, decimal lng, decimal? speed, decimal? heading);

    }
}
