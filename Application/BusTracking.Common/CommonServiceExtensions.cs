using BusTracking.Common.Data;
using BusTracking.Common.Interfaces;
using BusTracking.Common.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BusTracking.Common
{
    public static class CommonServiceExtensions
    {
        /// <summary>
        /// Registers DbContext + all shared services.
        /// Call from both BusTracking.Web and BusTracking.API Program.cs.
        /// </summary>
        public static IServiceCollection AddCommonServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // ── EF Core (SQL Server — uses manual DB, no migrations) ──────
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sql => sql.EnableRetryOnFailure(3)));

            // ── Infrastructure ────────────────────────────────────────────
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IPasswordService, PasswordService>();
            services.AddScoped<IEmailService, EmailService>();

            // ── Business Services ─────────────────────────────────────────
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IBusService, BusService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IFeedbackService, FeedbackService>();

            return services;
        }
    }
}
