namespace BusTracking.Web.Helpers
{
    public static class PermissionHelper
    {
        public static bool Can(ClaimsPrincipal user, string permissionKey, HttpContext? httpContext = null)
        {
            if (user == null || user.Identity?.IsAuthenticated != true)
                return false;

            // SuperAdmin ALWAYS has full, unrestricted access to all features
            if (user.IsSuperAdmin()) 
                return true;

            // Check permission claim from cookie session
            if (user.HasClaim("permission", permissionKey))
                return true;

            // Dynamic live check if claim is not in active cookie session
            if (httpContext != null)
            {
                var db = httpContext.RequestServices.GetService<AppDbContext>();
                var userIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (db != null && int.TryParse(userIdStr, out var userId))
                {
                    var hasPerm = db.SubAdminPermissions
                        .Any(sp => sp.UserId == userId && sp.Permission.PermissionKey == permissionKey);
                    if (hasPerm) return true;
                }
            }

            return false;
        }
    }
}
