namespace BusTracking.Mobile.Services
{
    public class ClassMappingService : IClassMappingService
    {
        private readonly IApiService _api;
        public ClassMappingService(IApiService api) => _api = api;

        public async Task<List<ClassMappingItem>> GetAllAsync(int? academicYearId = null, int? standardId = null, bool isCoordinator = false)
        {
            var baseEndpoint = isCoordinator ? Constants.Coordinator.ClassMapping : Constants.Admin.ClassMapping;
            var url = $"{baseEndpoint}?";
            if (academicYearId.HasValue && academicYearId.Value > 0) url += $"academicYearId={academicYearId.Value}&";
            if (standardId.HasValue && standardId.Value > 0) url += $"standardId={standardId.Value}&";

            var r = await _api.GetAsync<List<ClassMappingItem>>(url.TrimEnd('&', '?'));
            return r.Data ?? new List<ClassMappingItem>();
        }

        public Task<ApiResponse<object>> AssignAsync(AssignClassMappingRequest req, bool isCoordinator = false)
        {
            var endpoint = isCoordinator ? Constants.Coordinator.ClassMapping : Constants.Admin.ClassMapping;
            return _api.PostAsync<object>(endpoint, req);
        }

        public Task<ApiResponse<object>> DeleteAsync(int id, bool isCoordinator = false)
        {
            var endpoint = isCoordinator ? Constants.Coordinator.ClassMappingDelete : Constants.Admin.ClassMappingDelete;
            return _api.DeleteAsync<object>(string.Format(endpoint, id));
        }
    }
}
