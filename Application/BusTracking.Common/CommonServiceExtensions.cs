namespace BusTracking.Common;

public static class CommonServiceExtensions
{
    /// <summary>
    /// Registers DbContext + all shared services.
    /// Call from both BusTracking.Web and BusTracking.API Program.cs.
    /// </summary>
    public static IServiceCollection AddCommonServices(this IServiceCollection services, IConfiguration configuration)
    {
        // ── EF Core (SQL Server — uses manual DB, no migrations) ──────
        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"), sql => sql.EnableRetryOnFailure(3)));

        // ── Infrastructure ────────────────────────────────────────────
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IEmailService, EmailService>();

        // ── Business Services ─────────────────────────────────────────
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ISubAdminService, SubAdminService>();
        services.AddScoped<IRouteService, RouteService>();
        services.AddScoped<IBusService, BusService>();
        services.AddScoped<IDriverService, DriverService>();
        services.AddScoped<IParentService, ParentService>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<ITripService, TripService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IFeedbackService, FeedbackService>();

        // Extended services
        services.AddScoped<IStudentSearchService, StudentSearchService>();
        services.AddScoped<IDriverExtService, DriverExtService>();
        services.AddScoped<IStudentExtService, StudentExtService>();
        services.AddScoped<IParentExtService, ParentExtService>();
        services.AddScoped<ISubAdminExtService, SubAdminExtService>();

        // App Configuration
        services.AddScoped<IAppConfigService, AppConfigService>();

        return services;
    }
}