namespace BusTracking.Mobile.Services
{
    public class AcademicYearService : IAcademicYearService
    {
        private readonly IApiService _api;

        public AcademicYearService(IApiService api)
        {
            _api = api;
        }

        private string BaseUrl(bool isCoordinator) => isCoordinator ? "api/coordinator/academicyears" : "api/admin/academicyears";

        public async Task<List<AcademicYearItem>> GetAcademicYearsAsync(bool isCoordinator = false)
        {
            var res = await _api.GetAsync<List<AcademicYearItem>>(BaseUrl(isCoordinator));
            return res.Success && res.Data != null ? res.Data : new List<AcademicYearItem>();
        }

        public async Task<AcademicYearItem?> GetActiveAcademicYearAsync(bool isCoordinator = false)
        {
            var res = await _api.GetAsync<AcademicYearItem>($"{BaseUrl(isCoordinator)}/active");
            return res.Success ? res.Data : null;
        }

        public async Task<ApiResponse<AcademicYearItem>> CreateAcademicYearAsync(AcademicYearItem item, bool isCoordinator = false)
        {
            return await _api.PostAsync<AcademicYearItem>(BaseUrl(isCoordinator), item);
        }

        public async Task<ApiResponse<AcademicYearItem>> UpdateAcademicYearAsync(AcademicYearItem item, bool isCoordinator = false)
        {
            return await _api.PutAsync<AcademicYearItem>($"{BaseUrl(isCoordinator)}/{item.AcademicYearId}", item);
        }

        public async Task<ApiResponse<bool>> SetActiveAcademicYearAsync(int academicYearId, bool isCoordinator = false)
        {
            return await _api.PostAsync<bool>($"{BaseUrl(isCoordinator)}/{academicYearId}/set-active", new { });
        }

        public async Task<ApiResponse<bool>> ToggleStatusAsync(int academicYearId, bool isCoordinator = false)
        {
            return await _api.PostAsync<bool>($"{BaseUrl(isCoordinator)}/{academicYearId}/toggle-status", new { });
        }
    }
}
