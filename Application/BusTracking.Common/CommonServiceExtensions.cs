namespace BusTracking.Common;

public static class CommonServiceExtensions
{
    public static IServiceCollection AddCommonServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.EnableRetryOnFailure(3)));

        // Required for ImageService to read the live request URL
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // ImageService uses IWebHostEnvironment + IHttpContextAccessor — both auto-injected
        services.AddScoped<IImageService, ImageService>();
        services.AddScoped<ILogService, LogService>();

        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ISubAdminService, SubAdminService>();
        services.AddScoped<IRouteService, RouteService>();
        services.AddScoped<IBusService, BusService>();
        services.AddScoped<IBusTypeService, BusTypeService>();
        services.AddScoped<IDriverService, DriverService>();
        services.AddScoped<IParentService, ParentService>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<ITripService, TripService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IFeedbackService, FeedbackService>();
        services.AddScoped<IStudentSearchService, StudentSearchService>();
        services.AddScoped<IDriverExtService, DriverExtService>();
        services.AddScoped<IStudentExtService, StudentExtService>();
        services.AddScoped<IParentExtService, ParentExtService>();
        services.AddScoped<ISubAdminExtService, SubAdminExtService>();
        services.AddScoped<IAppConfigService, AppConfigService>();
        services.AddScoped<IStandardService, StandardService>();
        services.AddScoped<IDriverTripWebService, DriverTripWebService>();

        return services;
    }
}
