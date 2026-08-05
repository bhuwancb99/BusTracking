namespace BusTracking.Mobile.Interfaces
{
    public interface ISubjectService
    {
        Task<PagedResult<SubjectItem>> GetAllAsync(string? search = null, int page = 1, bool isCoordinator = false);
        Task<SubjectItem?> GetByIdAsync(int id, bool isCoordinator = false);
        Task<ApiResponse<object>> CreateAsync(CreateSubjectRequest req, bool isCoordinator = false);
        Task<ApiResponse<object>> UpdateAsync(int id, UpdateSubjectRequest req, bool isCoordinator = false);
        Task<ApiResponse<object>> DeleteAsync(int id, bool isCoordinator = false);
        Task<ApiResponse<object>> ToggleAsync(int id, bool isCoordinator = false);
    }
}
