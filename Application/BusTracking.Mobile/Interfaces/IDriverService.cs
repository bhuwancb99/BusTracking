namespace BusTracking.Mobile.Interfaces
{
    public interface IDriverService
    {
        Task<PagedResult<DriverItem>> GetAllAsync(string? search = null, int page = 1, string? status = "Active");
        Task<List<DriverItem>> GetAllForFormAsync(string? search = null);
        Task<DriverItem?> GetByIdAsync(int id);
        Task<ApiResponse<object>> CreateAsync(CreateDriverRequest req);
        Task<ApiResponse<object>> UpdateAsync(int id, UpdateDriverRequest req);
        Task<ApiResponse<object>> DeleteAsync(int id);
        Task<ApiResponse<object>> ToggleAsync(int id);
        Task<ApiResponse<ResetPasswordResult>> ResetPasswordAsync(int id);
        Task<List<DropdownItem>> GetDropdownAsync(string? search = null);
        Task<List<DriverNotificationItem>> GetAllNotificationAsync();
        Task<ApiResponse<object>> MarkReadAsync(int notificationId);
        Task<ApiResponse<object>> MarkAllReadAsync();
    }
}