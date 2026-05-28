namespace BusTracking.Mobile.Interfaces
{
    public interface IBusService
    {
        Task<List<BusItem>> GetAllAsync(string? search = null, int page = 1);
        Task<BusItem?> GetByIdAsync(int id);
        Task<ApiResponse<object>> CreateAsync(CreateBusRequest req);
        Task<ApiResponse<object>> UpdateAsync(int id, UpdateBusRequest req);
        Task<ApiResponse<object>> DeleteAsync(int id);
        Task<ApiResponse<object>> ToggleAsync(int id);
        Task<List<DropdownItem>> GetDropdownAsync(string? search = null);
    }
}
