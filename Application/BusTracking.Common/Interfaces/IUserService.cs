namespace BusTracking.Common.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<UserProfileDto>> GetProfileAsync(int userId);
        Task<ApiResponse<bool>> UpdateProfileAsync(int userId, UpdateProfileDto dto);
    }
}
