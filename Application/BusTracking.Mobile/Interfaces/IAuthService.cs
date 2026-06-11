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

        /// <summary>
        /// Updates ProfileImageUrl in the in-memory session and local DB.
        /// Call this after a photo upload or removal so the flyout avatar reflects
        /// the change immediately without requiring a re-login.
        /// </summary>
        Task RefreshProfileImageAsync(string? newUrl);
    }
}
