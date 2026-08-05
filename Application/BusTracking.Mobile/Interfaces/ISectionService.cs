namespace BusTracking.Mobile.Interfaces
{
    public interface ISectionService
    {
        Task<List<SectionItem>> GetByStandardAsync(int standardId, bool isCoordinator = false, bool isAdmin = false);
        Task<ApiResponse<object>> CreateAsync(CreateSectionRequest req, bool isCoordinator = false);
        Task<ApiResponse<object>> DeleteAsync(int id, bool isCoordinator = false);
    }
}
