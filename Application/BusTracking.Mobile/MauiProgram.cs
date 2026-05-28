namespace BusTracking.Mobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    //fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            RegisterServices(builder.Services);
            RegisterViewModels(builder.Services);
            RegisterViews(builder.Services);

            return builder.Build();
        }

        // ── Services ──────────────────────────────────────────────────────────
        private static void RegisterServices(IServiceCollection s)
        {
            // Infrastructure — Singletons (one instance for app lifetime)
            s.AddSingleton<LocalDatabase>();
            s.AddSingleton<ICacheService, CacheService>();
            s.AddSingleton<IApiService, ApiService>();
            s.AddSingleton<INavigationService, NavigationService>();

            // Auth — Singleton (holds current user in memory)
            s.AddSingleton<IAuthService, AuthService>();

            // Domain services — Transient (new per use, stateless)
            s.AddTransient<IAppConfigService, AppConfigService>();
            s.AddTransient<IDashboardService, DashboardService>();
            s.AddTransient<IBusService, BusService>();
            s.AddTransient<IDriverService, DriverService>();
            s.AddTransient<IStudentService, StudentService>();
            s.AddTransient<IParentService, ParentService>();
            s.AddTransient<ICoordinatorService, CoordinatorService>();
            s.AddTransient<ITripService, TripService>();
            s.AddTransient<IRouteService, RouteService>();
            s.AddTransient<IAdminConfigService, AdminConfigService>();
            s.AddTransient<IDriverTripService, DriverTripService>();
        }

        // ── ViewModels ────────────────────────────────────────────────────────
        private static void RegisterViewModels(IServiceCollection s)
        {
            // Auth
            s.AddTransient<LoginViewModel>();
            s.AddTransient<ChangePasswordViewModel>();

            // SuperAdmin
            s.AddTransient<AdminDashboardViewModel>();
            s.AddTransient<AdminBusListViewModel>();
            s.AddTransient<AdminBusFormViewModel>();
            s.AddTransient<AdminDriverListViewModel>();
            s.AddTransient<AdminStudentListViewModel>();
            s.AddTransient<AdminParentListViewModel>();
            s.AddTransient<AdminCoordinatorListViewModel>();
            s.AddTransient<AdminCoordinatorFormViewModel>();
            s.AddTransient<AdminTripListViewModel>();
            s.AddTransient<AdminConfigListViewModel>();
            s.AddTransient<AdminConfigFormViewModel>();

            // Coordinator
            s.AddTransient<CoordinatorDashboardViewModel>();
            s.AddTransient<CoordBusListViewModel>();
            s.AddTransient<CoordTripListViewModel>();
            s.AddTransient<CoordStudentListViewModel>();
            s.AddTransient<CoordParentListViewModel>();
            s.AddTransient<CoordDriverListViewModel>();
            s.AddTransient<CoordRouteListViewModel>();

            // Driver
            s.AddTransient<DriverDashboardViewModel>();
            s.AddTransient<DriverTripListViewModel>();
            s.AddTransient<DriverTrackingViewModel>();

            // Parent
            s.AddTransient<ParentDashboardViewModel>();
            s.AddTransient<ParentTrackingViewModel>();

            // Student
            s.AddTransient<StudentDashboardViewModel>();
            s.AddTransient<StudentTrackingViewModel>();
            s.AddTransient<StudentAvailabilityViewModel>();
        }

        // ── Views (Pages) ─────────────────────────────────────────────────────
        private static void RegisterViews(IServiceCollection s)
        {
            // Auth
            s.AddTransient<LoginPage>();
            s.AddTransient<ChangePasswordPage>();

            // SuperAdmin
            s.AddTransient<AdminDashboardPage>();
            s.AddTransient<AdminBusListPage>();
            s.AddTransient<AdminBusFormPage>();
            s.AddTransient<AdminDriverListPage>();
            s.AddTransient<AdminStudentListPage>();
            s.AddTransient<AdminParentListPage>();
            s.AddTransient<AdminCoordinatorListPage>();
            s.AddTransient<AdminCoordinatorFormPage>();
            s.AddTransient<AdminTripListPage>();
            s.AddTransient<AdminConfigListPage>();
            s.AddTransient<AdminConfigFormPage>();

            // Coordinator
            s.AddTransient<CoordinatorDashboardPage>();
            s.AddTransient<CoordBusListPage>();
            s.AddTransient<CoordTripListPage>();
            s.AddTransient<CoordStudentListPage>();
            s.AddTransient<CoordParentListPage>();
            s.AddTransient<CoordDriverListPage>();
            s.AddTransient<CoordRouteListPage>();

            // Driver
            s.AddTransient<DriverDashboardPage>();
            s.AddTransient<DriverTripListPage>();
            s.AddTransient<DriverTrackingPage>();

            // Parent
            s.AddTransient<ParentDashboardPage>();
            s.AddTransient<ParentTrackingPage>();

            // Student
            s.AddTransient<StudentDashboardPage>();
            s.AddTransient<StudentTrackingPage>();
            s.AddTransient<StudentAvailabilityPage>();

            // Common
            s.AddTransient<MaintenancePage>();
        }
    }
}
