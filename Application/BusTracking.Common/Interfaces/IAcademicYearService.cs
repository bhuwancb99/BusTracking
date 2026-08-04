namespace BusTracking.Common.Interfaces;

public interface IAcademicYearService
{
    Task<List<AcademicYearDto>> GetAcademicYearsAsync(int schoolId, bool activeOnly = false);
    Task<AcademicYearDto?> GetByIdAsync(int academicYearId);
    Task<AcademicYearDto?> GetActiveSessionAsync(int schoolId);
    Task<OperationResult<AcademicYearDto>> CreateAcademicYearAsync(CreateAcademicYearRequest request, string? createdBy);
    Task<OperationResult<AcademicYearDto>> UpdateAcademicYearAsync(UpdateAcademicYearRequest request, string? updatedBy);
    Task<OperationResult<bool>> SetActiveAcademicYearAsync(int schoolId, int academicYearId, string? updatedBy);
    Task<OperationResult<bool>> ToggleAcademicYearStatusAsync(int academicYearId, string? updatedBy);
}
