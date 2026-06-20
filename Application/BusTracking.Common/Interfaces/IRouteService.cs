namespace BusTracking.Common.Interfaces
{
    public interface IRouteService
    {
        Task<ApiResponse<PagedResult<RouteListDto>>> GetAllAsync(int page, string? search, string? status = "Active");
        Task<int> GetListPageSizeAsync();
        Task<List<RouteListDto>> GetDropdownAsync(string? search = null);
        Task<ApiResponse<bool>> ToggleActiveAsync(int routeId);
        Task<ApiResponse<RouteDetailDto>> GetByIdAsync(int routeId);
        Task<ApiResponse<bool>> CreateAsync(CreateRouteDto dto, int createdBy);
        Task<ApiResponse<bool>> UpdateAsync(int routeId, UpdateRouteDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int routeId);
        Task<ApiResponse<bool>> AddStopAsync(CreateStopDto dto);
        Task<ApiResponse<bool>> DeleteStopAsync(int stopId);
        Task<ApiResponse<List<StopDto>>> GetStopsByRouteAsync(int routeId);
        Task<ApiResponse<List<StopDto>>> GetStopsByBusAsync(int busId);


    }
}
