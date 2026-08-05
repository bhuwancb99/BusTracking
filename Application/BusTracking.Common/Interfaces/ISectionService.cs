namespace BusTracking.Common.Interfaces
{
    public interface ISectionService
    {
        Task<ApiResponse<List<SectionDto>>> GetSectionsByStandardAsync(int standardId);
        Task<ApiResponse<SectionDto>> GetByIdAsync(int sectionId);
        Task<ApiResponse<SectionDto>> CreateAsync(CreateSectionDto dto);
        Task<ApiResponse<bool>> UpdateAsync(int sectionId, UpdateSectionDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int sectionId);
        Task<ApiResponse<bool>> ToggleActiveAsync(int sectionId);
        Task<ApiResponse<SectionDto>> EnsureDefaultSectionAAsync(int standardId, int schoolId);
    }
}
