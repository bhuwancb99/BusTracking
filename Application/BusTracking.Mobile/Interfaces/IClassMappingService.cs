namespace BusTracking.Mobile.Interfaces
{
    public interface IClassMappingService
    {
        Task<List<ClassMappingItem>> GetAllAsync(int? academicYearId = null, int? standardId = null, bool isCoordinator = false);
        Task<ApiResponse<object>> AssignAsync(AssignClassMappingRequest req, bool isCoordinator = false);
        Task<ApiResponse<object>> DeleteAsync(int id, bool isCoordinator = false);
    }
}
