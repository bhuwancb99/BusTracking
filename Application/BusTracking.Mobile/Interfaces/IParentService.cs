namespace BusTracking.Mobile.Interfaces
{
    public interface IParentService
    {
        Task<List<ParentItem>> GetAllAsync(string? search = null, int page = 1);
        Task<ParentItem?> GetByIdAsync(int id);
        Task<ApiResponse<object>> CreateAsync(CreateParentRequest req);
        Task<ApiResponse<object>> UpdateAsync(int id, UpdateParentRequest req);
        Task<ApiResponse<object>> DeleteAsync(int id);
        Task<ApiResponse<object>> ToggleAsync(int id);
        Task<ApiResponse<object>> ResetPasswordAsync(int id);
        Task<object?> GetDashboardAsync();
        Task<TrackingData?> TrackChildBusAsync(int studentId);
    }
}
