namespace BusTracking.Mobile.Interfaces
{
    public interface ISectionService
    {
        Task<List<SectionItem>> GetByStandardAsync(int standardId, bool isCoordinator = false, bool isAdmin = false);
        Task<ApiResponse<SectionItem>> GetByIdAsync(int id, bool isCoordinator = false);
        Task<ApiResponse<object>> CreateAsync(CreateSectionRequest req, bool isCoordinator = false);
        Task<ApiResponse<object>> UpdateAsync(int id, UpdateSectionRequest req, bool isCoordinator = false);
        Task<ApiResponse<object>> DeleteAsync(int id, bool isCoordinator = false);
        Task<ApiResponse<bool>> ToggleActiveAsync(int id, bool isCoordinator = false);
    }
}
