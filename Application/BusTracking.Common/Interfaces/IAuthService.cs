namespace BusTracking.Common.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto dto);
        Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordDto dto);
        Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordDto dto);
        Task<ApiResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordDto dto);
    }
}
