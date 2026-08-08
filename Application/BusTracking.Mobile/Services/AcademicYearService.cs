namespace BusTracking.Mobile.Services
{
    public class AcademicYearService : IAcademicYearService
    {
        private readonly IApiService _api;

        public AcademicYearService(IApiService api)
        {
            _api = api;
        }

        public async Task<List<AcademicYearItem>> GetAcademicYearsAsync(bool isCoordinator = false, bool isAdmin = false)
        {
            string url = isAdmin ? Constants.AcademicYear.AdminBase :
                         isCoordinator ? Constants.AcademicYear.CoordBase :
                         Constants.Teacher.AcademicYears;

            var res = await _api.GetAsync<List<AcademicYearItem>>(url);
            return res.Success && res.Data != null ? res.Data : new List<AcademicYearItem>();
        }

        public async Task<AcademicYearItem?> GetActiveAcademicYearAsync(bool isCoordinator = false, bool isAdmin = false)
        {
            var res = await _api.GetAsync<AcademicYearItem>(Constants.AcademicYear.Active(isCoordinator));
            return res.Success ? res.Data : null;
        }

        public async Task<ApiResponse<AcademicYearItem>> CreateAcademicYearAsync(AcademicYearItem item, bool isCoordinator = false)
        {
            return await _api.PostAsync<AcademicYearItem>(Constants.AcademicYear.Base(isCoordinator), item);
        }

        public async Task<ApiResponse<AcademicYearItem>> UpdateAcademicYearAsync(AcademicYearItem item, bool isCoordinator = false)
        {
            return await _api.PutAsync<AcademicYearItem>(Constants.AcademicYear.ById(isCoordinator, item.AcademicYearId), item);
        }

        public async Task<ApiResponse<bool>> SetActiveAcademicYearAsync(int academicYearId, bool isCoordinator = false)
        {
            return await _api.PostAsync<bool>(Constants.AcademicYear.SetActive(isCoordinator, academicYearId), new { });
        }

        public async Task<ApiResponse<bool>> ToggleStatusAsync(int academicYearId, bool isCoordinator = false)
        {
            return await _api.PostAsync<bool>(Constants.AcademicYear.ToggleStatus(isCoordinator, academicYearId), new { });
        }
    }
}
