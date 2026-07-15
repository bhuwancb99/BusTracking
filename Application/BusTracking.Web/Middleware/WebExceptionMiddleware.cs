namespace BusTracking.Web.Middleware
{
    public class WebExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public WebExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ILogService logService)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                try
                {
                    var routeData = context.GetRouteData();
                    string? controller = routeData?.Values["controller"]?.ToString();
                    string? action = routeData?.Values["action"]?.ToString();

                    int? userId = null;
                    var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (int.TryParse(userIdClaim, out var parsedId))
                        userId = parsedId;

                    string? username = context.User.FindFirst(ClaimTypes.Email)?.Value ?? context.User.Identity?.Name;
                    string? role = context.User.FindFirst(ClaimTypes.Role)?.Value;

                    await logService.LogAsync(
                        platform: "WEB",
                        exceptionMessage: ex.Message,
                        stackTrace: ex.StackTrace,
                        requestUrl: $"{context.Request.Path}{context.Request.QueryString}",
                        userId: userId,
                        username: username,
                        role: role,
                        moduleName: controller,
                        actionName: action,
                        additionalDetails: "Web application unhandled exception"
                    );
                }
                catch
                {
                    // Prevent logger exceptions from masking original exception
                }

                throw;
            }
        }
    }
}
