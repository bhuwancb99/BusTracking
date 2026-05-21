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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
                ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                ctx.Response.ContentType = "application/json";
                var response = ApiResponse<object>.Fail("An unexpected error occurred. Please try again.");
                await ctx.Response.WriteAsync(JsonSerializer.Serialize(response,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
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
