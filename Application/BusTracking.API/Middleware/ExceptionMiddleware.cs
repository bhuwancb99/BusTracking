namespace BusTracking.API.Middleware
{
    // ─── Global exception handler ─────────────────────────────────────────
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        { _next = next; _logger = logger; }

        public async Task InvokeAsync(HttpContext ctx)
        {
            try
            {
                await _next(ctx);
            }
            catch (UnauthorizedAccessException ex)
            {
                await LogToDbAsync(ctx, ex, "Permission check failure");
                _logger.LogWarning("Permission denied: {Message}", ex.Message);
                ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                ctx.Response.ContentType = "application/json";
                var response = ApiResponse<object>.Fail(ex.Message.Replace("Missing permission: ", "You don't have permission to access this feature."));
                await ctx.Response.WriteAsync(JsonSerializer.Serialize(response,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
            }
            catch (Exception ex)
            {
                await LogToDbAsync(ctx, ex, "API unhandled exception");
                _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
                ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                ctx.Response.ContentType = "application/json";
                var response = ApiResponse<object>.Fail("An unexpected error occurred. Please try again.");
                await ctx.Response.WriteAsync(JsonSerializer.Serialize(response,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
            }
        }

        private async Task LogToDbAsync(HttpContext ctx, Exception ex, string details)
        {
            try
            {
                var logService = ctx.RequestServices.GetRequiredService<ILogService>();
                var routeData = ctx.GetRouteData();
                string? controller = routeData?.Values["controller"]?.ToString();
                string? action = routeData?.Values["action"]?.ToString();

                int? userId = null;
                var userIdClaim = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out var parsedId))
                    userId = parsedId;

                string? username = ctx.User.FindFirst(ClaimTypes.Email)?.Value ?? ctx.User.Identity?.Name;
                string? role = ctx.User.FindFirst(ClaimTypes.Role)?.Value;

                await logService.LogAsync(
                    platform: "API",
                    exceptionMessage: ex.Message,
                    stackTrace: ex.StackTrace,
                    requestUrl: $"{ctx.Request.Path}{ctx.Request.QueryString}",
                    userId: userId,
                    username: username,
                    role: role,
                    moduleName: controller,
                    actionName: action,
                    additionalDetails: details
                );
            }
            catch
            {
                // Prevent issues inside DB logging from hiding original exception
            }
        }
    }

    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        // ─── Request logging middleware ───────────────────────────────────────
        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        { _next = next; _logger = logger; }

        public async Task InvokeAsync(HttpContext ctx)
        {
            var start = DateTime.UtcNow;
            await _next(ctx);
            var elapsed = (DateTime.UtcNow - start).TotalMilliseconds;
            _logger.LogInformation("{Method} {Path} → {StatusCode} ({Elapsed:0}ms)",
                ctx.Request.Method, ctx.Request.Path, ctx.Response.StatusCode, elapsed);
        }
    }

    // ─── Extension to register both ──────────────────────────────────────
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder app)
            => app.UseMiddleware<ExceptionMiddleware>();

        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
            => app.UseMiddleware<RequestLoggingMiddleware>();
    }
}
