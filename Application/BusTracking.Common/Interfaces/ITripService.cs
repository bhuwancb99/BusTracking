using BusTracking.Common.DTOs.Common;
using BusTracking.Common.DTOs.Trip;

namespace BusTracking.Common.Interfaces
{
    public interface ITripService
    {
        Task<ApiResponse<PagedResult<TripListDto>>> GetAllAsync(int page, int pageSize, string? busId);
        Task<ApiResponse<List<StudentTripStatusDto>>> GetTripStudentsAsync(int tripId);
        Task<ApiResponse<BusLocationDto?>> GetLatestLocationAsync(int tripId);
    }
}
