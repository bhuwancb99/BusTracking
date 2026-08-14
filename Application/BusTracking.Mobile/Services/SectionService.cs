namespace BusTracking.Mobile.Services
{
    public class SectionService : ISectionService
    {
        private readonly IApiService _api;
        private readonly IAuthService _auth;

        public SectionService(IApiService api, IAuthService auth)
        {
            _api = api;
            _auth = auth;
        }

        public async Task<List<SectionItem>> GetByStandardAsync(int standardId, bool isCoordinator = false, bool isAdmin = false)
        {
            var user = await _auth.GetCurrentUserAsync();
            string role = user?.Role ?? "";

            string endpoint = isAdmin ? Constants.Admin.SectionsByStandard :
                              isCoordinator ? Constants.Coordinator.SectionsByStandard :
                              role switch
                              {
                                  Constants.Roles.SuperAdmin => Constants.Admin.SectionsByStandard,
                                  Constants.Roles.BusCoordinator => Constants.Coordinator.SectionsByStandard,
                                  Constants.Roles.Teacher => Constants.Teacher.SectionsByStandard,
                                  _ => Constants.Admin.SectionsByStandard
                              };

            var r = await _api.GetAsync<List<SectionItem>>(string.Format(endpoint, standardId));
            return r.Data ?? new List<SectionItem>();
        }

        public async Task<ApiResponse<SectionItem>> GetByIdAsync(int id, bool isCoordinator = false)
        {
            var user = await _auth.GetCurrentUserAsync();
            string role = user?.Role ?? "";
            bool isCoord = isCoordinator || role == Constants.Roles.BusCoordinator;
            var endpoint = isCoord ? Constants.Coordinator.SectionById : Constants.Admin.SectionById;
            return await _api.GetAsync<SectionItem>(string.Format(endpoint, id));
        }

        public async Task<ApiResponse<object>> CreateAsync(CreateSectionRequest req, bool isCoordinator = false)
        {
            var user = await _auth.GetCurrentUserAsync();
            string role = user?.Role ?? "";
            bool isCoord = isCoordinator || role == Constants.Roles.BusCoordinator;
            var endpoint = isCoord ? Constants.Coordinator.Sections : Constants.Admin.Sections;
            return await _api.PostAsync<object>(endpoint, req);
        }

        public async Task<ApiResponse<object>> UpdateAsync(int id, UpdateSectionRequest req, bool isCoordinator = false)
        {
            var user = await _auth.GetCurrentUserAsync();
            string role = user?.Role ?? "";
            bool isCoord = isCoordinator || role == Constants.Roles.BusCoordinator;
            var endpoint = isCoord ? Constants.Coordinator.SectionById : Constants.Admin.SectionById;
            return await _api.PutAsync<object>(string.Format(endpoint, id), req);
        }

        public async Task<ApiResponse<object>> DeleteAsync(int id, bool isCoordinator = false)
        {
            var user = await _auth.GetCurrentUserAsync();
            string role = user?.Role ?? "";
            bool isCoord = isCoordinator || role == Constants.Roles.BusCoordinator;
            var endpoint = isCoord ? Constants.Coordinator.SectionById : Constants.Admin.SectionById;
            return await _api.DeleteAsync<object>(string.Format(endpoint, id));
        }

        public async Task<ApiResponse<bool>> ToggleActiveAsync(int id, bool isCoordinator = false)
        {
            var user = await _auth.GetCurrentUserAsync();
            string role = user?.Role ?? "";
            bool isCoord = isCoordinator || role == Constants.Roles.BusCoordinator;
            var endpoint = isCoord ? Constants.Coordinator.SectionToggle : Constants.Admin.SectionToggle;
            return await _api.PostAsync<bool>(string.Format(endpoint, id), new { });
        }
    }
}
