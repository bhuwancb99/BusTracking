namespace BusTracking.Mobile.Interfaces
{
    public interface IBusTypeService
    {
        Task<List<BusTypeItem>> GetAllAsync(string? search = null);
        Task<ApiResponse<object>> CreateAsync(string name);
        Task<ApiResponse<object>> UpdateAsync(int id, string name);
        Task<ApiResponse<object>> DeleteAsync(int id);
    }
}
