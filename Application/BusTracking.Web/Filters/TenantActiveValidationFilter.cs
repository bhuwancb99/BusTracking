namespace BusTracking.Web.Filters
{
    public class TenantActiveValidationFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var controllerName = context.ActionDescriptor.RouteValues["controller"];
            if (string.Equals(controllerName, "Auth", StringComparison.OrdinalIgnoreCase))
            {
                await next();
                return;
            }

            var httpContext = context.HttpContext;
            if (httpContext.User.Identity?.IsAuthenticated == true)
            {
                var db = httpContext.RequestServices.GetRequiredService<AppDbContext>();
                
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out var userId))
                {
                    var roleClaim = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;
                    if (roleClaim != "SystemAdmin")
                    {
                        var user = await db.Users
                            .IgnoreQueryFilters()
                            .Include(u => u.Role)
                            .FirstOrDefaultAsync(u => u.UserId == userId);

                        bool deactivate = false;

                        if (user == null || !user.IsActive)
                        {
                            deactivate = true;
                        }
                        else if (user.SchoolId.HasValue)
                        {
                            var school = await db.Schools.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.SchoolId == user.SchoolId.Value);
                            if (school == null || !school.IsActive)
                            {
                                deactivate = true;
                            }
                            else if (user.Role.RoleName != AppConstants.RoleSuperAdmin)
                            {
                                var hasActiveSuperAdmin = await db.Users
                                    .IgnoreQueryFilters()
                                    .AnyAsync(u => u.SchoolId == user.SchoolId && u.Role.RoleId == 1 && u.IsActive);
                                if (!hasActiveSuperAdmin)
                                {
                                    deactivate = true;
                                }
                            }
                        }

                        if (deactivate)
                        {
                            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                            
                            var controller = context.Controller as Controller;
                            if (controller != null)
                            {
                                controller.TempData["ErrorMessage"] = "Your account has been deactivated. Please contact your System Administrator for assistance.";
                            }
                            
                            context.Result = new RedirectToActionResult("Login", "Auth", new { area = "" });
                            return;
                        }
                    }
                }
            }

            await next();
        }
    }
}
