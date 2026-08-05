namespace BusTracking.Mobile.Services
{
    public class SectionService : ISectionService
    {
        private readonly IApiService _api;
        public SectionService(IApiService api) => _api = api;

        public async Task<List<SectionItem>> GetByStandardAsync(int standardId, bool isCoordinator = false, bool isAdmin = false)
        {
            var endpoint = isCoordinator ? Constants.Coordinator.SectionsByStandard : Constants.Admin.SectionsByStandard;
            var r = await _api.GetAsync<List<SectionItem>>(string.Format(endpoint, standardId));
            return r.Data ?? new List<SectionItem>();
        }

        public Task<ApiResponse<object>> CreateAsync(CreateSectionRequest req, bool isCoordinator = false)
        {
            var endpoint = isCoordinator ? Constants.Coordinator.Sections : Constants.Admin.Sections;
            return _api.PostAsync<object>(endpoint, req);
        }

        public Task<ApiResponse<object>> DeleteAsync(int id, bool isCoordinator = false)
        {
            var endpoint = isCoordinator ? Constants.Coordinator.SectionById : Constants.Admin.SectionById;
            return _api.DeleteAsync<object>(string.Format(endpoint, id));
        }
    }
}
