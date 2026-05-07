using BusTracking.Common.DTOs.Common;
using BusTracking.Common.DTOs.Route;
using BusTracking.Common.DTOs.Stop;

namespace BusTracking.Common.Interfaces
{
    public interface IRouteService
    {
        Task<ApiResponse<PagedResult<RouteListDto>>> GetAllAsync(int page, int pageSize, string? search);
        Task<ApiResponse<RouteDetailDto>> GetByIdAsync(int routeId);
        Task<ApiResponse<bool>> CreateAsync(CreateRouteDto dto, int createdBy);
        Task<ApiResponse<bool>> UpdateAsync(int routeId, UpdateRouteDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int routeId);
        Task<ApiResponse<bool>> AddStopAsync(CreateStopDto dto);
        Task<ApiResponse<bool>> DeleteStopAsync(int stopId);
    }
}
