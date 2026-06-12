namespace BusTracking.Common.Interfaces
{
    public interface IBusTypeService
    {
        Task<ApiResponse<List<BusTypeDto>>> GetAllAsync();
        Task<ApiResponse<BusTypeDto>> GetByIdAsync(int id);
        Task<ApiResponse<bool>> CreateAsync(SaveBusTypeDto dto);
        Task<ApiResponse<bool>> UpdateAsync(int id, SaveBusTypeDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
        Task<ApiResponse<List<BusTypeDropdownDto>>> GetDropdownAsync();
    }
}
