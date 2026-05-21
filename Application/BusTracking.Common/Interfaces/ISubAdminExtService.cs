namespace BusTracking.Common.Interfaces
{
    public interface ISubAdminExtService
    {
        Task<ApiResponse<CreatedUserResultDto>> CreateExtAsync(CreateSubAdminExtDto dto, int createdBy);

        Task<ApiResponse<bool>> UpdateExtAsync(int userId, UpdateSubAdminExtDto dto);
    }

}
