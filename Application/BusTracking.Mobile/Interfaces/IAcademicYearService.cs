namespace BusTracking.Mobile.Interfaces
{
    public interface IAcademicYearService
    {
        Task<List<AcademicYearItem>> GetAcademicYearsAsync(bool isCoordinator = false);
        Task<AcademicYearItem?> GetActiveAcademicYearAsync(bool isCoordinator = false);
        Task<ApiResponse<AcademicYearItem>> CreateAcademicYearAsync(AcademicYearItem item, bool isCoordinator = false);
        Task<ApiResponse<AcademicYearItem>> UpdateAcademicYearAsync(AcademicYearItem item, bool isCoordinator = false);
        Task<ApiResponse<bool>> SetActiveAcademicYearAsync(int academicYearId, bool isCoordinator = false);
        Task<ApiResponse<bool>> ToggleStatusAsync(int academicYearId, bool isCoordinator = false);
    }
}
