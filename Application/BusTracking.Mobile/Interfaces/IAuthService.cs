using BusTracking.Mobile.Models.Auth;
using BusTracking.Mobile.Models.Common;

namespace BusTracking.Mobile.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<SessionUser>> LoginAsync(string email, string password);
        Task<SessionUser?> GetCurrentUserAsync();
        Task<bool> IsAuthenticatedAsync();
        Task LogoutAsync();
        Task<ApiResponse<bool>> ChangePasswordAsync(string current, string newPwd);
        Task<ApiResponse<bool>> ForgotPasswordAsync(string email);
        bool HasPermission(string permissionKey);
        string CurrentRole { get; }
    }
}
