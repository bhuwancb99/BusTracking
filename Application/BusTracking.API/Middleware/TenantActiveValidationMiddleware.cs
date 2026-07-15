namespace BusTracking.API.Middleware
{
    public class TenantActiveValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantActiveValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var db = context.RequestServices.GetRequiredService<AppDbContext>();

                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out var userId))
                {
                    // Fetch user and check active status, ignoring multi-tenant filters for security checks
                    var user = await db.Users
                        .IgnoreQueryFilters()
                        .Include(u => u.Role)
                        .FirstOrDefaultAsync(u => u.UserId == userId);

                    if (user == null || !user.IsActive)
                    {
                        await RespondDeactivatedAsync(context);
                        return;
                    }

                    if (user.SchoolId.HasValue)
                    {
                        // Check if school is active
                        var school = await db.Schools.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.SchoolId == user.SchoolId.Value);
                        if (school == null || !school.IsActive)
                        {
                            await RespondDeactivatedAsync(context);
                            return;
                        }

                        // For non-SuperAdmins, check if there is at least one active SuperAdmin in their school
                        if (user.Role.RoleName != AppConstants.RoleSuperAdmin)
                        {
                            var hasActiveSuperAdmin = await db.Users
                                .IgnoreQueryFilters()
                                .AnyAsync(u => u.SchoolId == user.SchoolId && u.Role.RoleId == 1 && u.IsActive);
                            if (!hasActiveSuperAdmin)
                            {
                                await RespondDeactivatedAsync(context);
                                return;
                            }
                        }
                    }
                }
            }

            await _next(context);
        }

        private static async Task RespondDeactivatedAsync(HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            context.Response.ContentType = "application/json";
            
            var response = ApiResponse<object>.Fail("Your account has been deactivated. Please contact your System Administrator for assistance.");
            await context.Response.WriteAsync(JsonSerializer.Serialize(response,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        }
    }

    public static class TenantActiveValidationMiddlewareExtensions
    {
        public static IApplicationBuilder UseTenantActiveValidation(this IApplicationBuilder app)
            => app.UseMiddleware<TenantActiveValidationMiddleware>();
    }
}
