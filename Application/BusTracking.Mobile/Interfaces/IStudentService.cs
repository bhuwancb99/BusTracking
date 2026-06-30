namespace BusTracking.Mobile.Interfaces
{
    public interface IStudentService
    {
        Task<PagedResult<StudentItem>> GetAllAsync(string? search = null, int page = 1, string? status = "Active");
        Task<StudentItem?> GetByIdAsync(int id);
        Task<ApiResponse<object>> CreateAsync(CreateStudentRequest req);
        Task<ApiResponse<object>> UpdateAsync(int id, UpdateStudentRequest req);
        Task<ApiResponse<object>> DeleteAsync(int id);
        Task<ApiResponse<object>> ToggleAsync(int id);
        Task<ApiResponse<ResetPasswordResult>> ResetPasswordAsync(int id);
        Task<List<StudentItem>> SearchAsync(string query);
        Task<TrackingData?> GetTrackingAsync();
        Task<ApiResponse<bool>> SetAvailabilityAsync(object req);
    }
}