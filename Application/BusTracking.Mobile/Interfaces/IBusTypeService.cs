namespace BusTracking.Mobile.Interfaces
{
    public interface IBusTypeService
    {
        Task<PagedResult<BusTypeItem>> GetAllAsync(string? search = null, int page = 1);
        Task<List<BusTypeItem>> GetDropdownAsync();
        Task<ApiResponse<object>> CreateAsync(string name);
        Task<ApiResponse<object>> UpdateAsync(int id, string name);
        Task<ApiResponse<object>> DeleteAsync(int id);
    }
}
