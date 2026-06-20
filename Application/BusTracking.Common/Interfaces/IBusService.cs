namespace BusTracking.Common.Interfaces
{
    public interface IBusService
    {
        Task<ApiResponse<PagedResult<BusListDto>>> GetAllAsync(int page, string? search, string? status);
        Task<int> GetListPageSizeAsync();
        Task<ApiResponse<BusListDto>> GetByIdAsync(int busId);
        Task<ApiResponse<bool>> CreateAsync(CreateBusDto dto, int createdBy);
        Task<ApiResponse<bool>> UpdateAsync(int busId, UpdateBusDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int busId);
        Task<ApiResponse<bool>> ToggleActiveAsync(int busId);
        Task<ApiResponse<bool>> AssignDriverAsync(AssignDriverToBusDto dto);
        Task<ApiResponse<bool>> AssignStudentAsync(int busId, int studentId);
        Task<ApiResponse<bool>> RemoveStudentAsync(int busId, int studentId);
        Task<ApiResponse<List<BusDropdownDto>>> GetDropdownAsync(string? search);
    }
}
