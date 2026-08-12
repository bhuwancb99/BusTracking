namespace BusTracking.Mobile.Services
{
    public class AcademicYearService : IAcademicYearService
    {
        private readonly IApiService _api;
        private readonly IAuthService _auth;

        public AcademicYearService(IApiService api, IAuthService auth)
        {
            _api = api;
            _auth = auth;
        }

        public async Task<List<AcademicYearItem>> GetAcademicYearsAsync(bool isCoordinator = false, bool isAdmin = false)
        {
            var user = await _auth.GetCurrentUserAsync();
            string role = user?.Role ?? "";

            string url = isAdmin ? Constants.AcademicYear.AdminBase :
                         isCoordinator ? Constants.AcademicYear.CoordBase :
                         role switch
                         {
                             Constants.Roles.Student => Constants.Student.AcademicYears,
                             Constants.Roles.Parent => Constants.Parent.AcademicYears,
                             Constants.Roles.Driver => Constants.Driver.AcademicYears,
                             _ => Constants.Teacher.AcademicYears
                         };

            var res = await _api.GetAsync<List<AcademicYearItem>>(url);
            return res.Success && res.Data != null ? res.Data : new List<AcademicYearItem>();
        }

        public async Task<AcademicYearItem?> GetActiveAcademicYearAsync(bool isCoordinator = false, bool isAdmin = false)
        {
            var years = await GetAcademicYearsAsync(isCoordinator, isAdmin);
            return years.FirstOrDefault(y => y.IsCurrent) ?? years.FirstOrDefault();
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
            var user = await _auth.GetCurrentUserAsync();
            string role = user?.Role ?? "";

            string url = isCoordinator ? Constants.AcademicYear.SetActive(true, academicYearId) :
                         role switch
                         {
                             Constants.Roles.Student => string.Format(Constants.Student.SessionSwitch, academicYearId),
                             Constants.Roles.Parent => string.Format(Constants.Parent.SessionSwitch, academicYearId),
                             Constants.Roles.Driver => string.Format(Constants.Driver.SessionSwitch, academicYearId),
                             _ => string.Format(Constants.Teacher.SessionSwitch, academicYearId)
                         };

            return await _api.PostAsync<bool>(url, new { });
        }

        public async Task<ApiResponse<bool>> ToggleStatusAsync(int academicYearId, bool isCoordinator = false)
        {
            return await _api.PostAsync<bool>(Constants.AcademicYear.ToggleStatus(isCoordinator, academicYearId), new { });
        }
    }
}
