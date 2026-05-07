using System.Security.Claims;

namespace BusTracking.Common.Helpers
{
    public static class ClaimsExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
            => int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

        public static string GetEmail(this ClaimsPrincipal user)
            => user.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

        public static string GetFullName(this ClaimsPrincipal user)
            => user.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

        public static string GetRole(this ClaimsPrincipal user)
            => user.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        public static bool IsInAnyRole(this ClaimsPrincipal user, params string[] roles)
            => roles.Any(user.IsInRole);

        public static bool IsSuperAdmin(this ClaimsPrincipal user)
            => user.IsInRole(AppConstants.RoleSuperAdmin);

        public static bool IsAdmin(this ClaimsPrincipal user)
            => user.IsInRole(AppConstants.RoleSuperAdmin) || user.IsInRole(AppConstants.RoleBusCoordinator);
    }
}
