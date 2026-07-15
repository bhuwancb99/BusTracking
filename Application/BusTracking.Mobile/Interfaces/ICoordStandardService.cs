namespace BusTracking.Mobile.Interfaces
{
    public interface ICoordStandardService
    {
        Task<PagedResult<StandardItem>> GetAllAsync(string? search = null, int page = 1);
        Task<StandardItem?> GetByIdAsync(int id);
        Task<ApiResponse<object>> CreateAsync(CreateStandardRequest req);
        Task<ApiResponse<object>> UpdateAsync(int id, UpdateStandardRequest req);
        Task<ApiResponse<object>> DeleteAsync(int id);
        Task<ApiResponse<object>> ToggleAsync(int id);
    }
}
