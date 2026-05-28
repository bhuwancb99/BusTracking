namespace BusTracking.Mobile.Interfaces
{
    public interface IRouteService
    {
        Task<List<RouteItem>> GetAllAsync();
        Task<List<StopItem>> GetStopsAsync(int routeId);
        Task<ApiResponse<object>> CreateAsync(CreateRouteRequest req);
        Task<ApiResponse<object>> UpdateAsync(int id, UpdateRouteRequest req);
        Task<ApiResponse<object>> DeleteAsync(int id);
    }
}
