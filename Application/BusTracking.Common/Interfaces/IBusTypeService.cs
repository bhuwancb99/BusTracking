namespace BusTracking.Common.Interfaces
{
    public interface IBusTypeService
    {
        Task<ApiResponse<PagedResult<BusTypeDto>>> GetAllAsync(string? search = null, int page = 1);
        Task<ApiResponse<BusTypeDto>> GetByIdAsync(int id);

        Task<ApiResponse<BusTypeDto>> CreateAsync(SaveBusTypeDto dto);
        Task<ApiResponse<bool>> UpdateAsync(int id, SaveBusTypeDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
        Task<ApiResponse<List<BusTypeDropdownDto>>> GetDropdownAsync();

        Task<int> GetListPageSizeAsync();
    }
}
