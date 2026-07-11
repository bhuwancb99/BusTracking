namespace BusTracking.Mobile.Interfaces
{
    public interface IRouteService
    {
        Task<PagedResult<RouteItem>> GetAllAsync(string? search = null, int page = 1, string? status = "Active");
        Task<List<RouteItem>> GetDropdownAsync(string? search = null);
        Task<RouteItem?> GetByIdAsync(int id);
        Task<List<StopItem>> GetStopsAsync(int routeId);
        Task<ApiResponse<object>> CreateAsync(CreateRouteRequest req);
        Task<ApiResponse<object>> UpdateAsync(int id, UpdateRouteRequest req);
        Task<ApiResponse<object>> DeleteAsync(int id);
        Task<ApiResponse<object>> AddStopAsync(CreateStopRequest req);
        Task<ApiResponse<object>> DeleteStopAsync(int stopId, int routeId);
        Task<ApiResponse<object>> ReorderStopsAsync(ReorderStopsRequest req);
    }
}
