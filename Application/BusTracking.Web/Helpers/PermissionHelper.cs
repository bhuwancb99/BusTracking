namespace BusTracking.Web.Helpers
{
    public static class PermissionHelper
    {
        public static bool Can(ClaimsPrincipal user, string permissionKey)
        {
            if (user.IsSuperAdmin()) 
                return true;
            return user.HasClaim("permission", permissionKey);
        }
    }
}
