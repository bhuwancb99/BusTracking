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
                .ConfigureSyncfusionToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif
            MauiControlsHandlers();
            RegisterRoutes();
            RegisterServices(builder.Services);
            RegisterViewModels(builder.Services);
            RegisterViews(builder.Services);

            var app = builder.Build();

            // Set up global unhandled exception handling
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                var ex = args.ExceptionObject as Exception;
                if (ex != null)
                {
                    var logService = IPlatformApplication.Current?.Services.GetService<IMobileLogService>();
                    logService?.LogExceptionAsync(ex, "Global", "UnhandledException").GetAwaiter().GetResult();
                }
            };

            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                var logService = IPlatformApplication.Current?.Services.GetService<IMobileLogService>();
                logService?.LogExceptionAsync(args.Exception, "Global", "UnobservedTaskException").GetAwaiter().GetResult();
                args.SetObserved();
            };

            return app;
        }

        // ── Route Registration ────────────────────────────────────────────────
        // Shell.GoToAsync("SomeRoute")  without "//" is PUSH navigation.
        // Push navigation requires the route to be registered via Routing.RegisterRoute().
        // ShellContent Route= in XAML only makes it reachable with "//SomeRoute" (absolute).
        // Every page that is pushed onto the nav stack from a list/detail/form
        // MUST appear here. Missing entries = crash on navigation.
        private static void RegisterRoutes()
        {
            // ── Auth ──────────────────────────────────────────────────────────
            Routing.RegisterRoute("ChangePassword", typeof(ChangePasswordPage));

            // ── Common ────────────────────────────────────────────────────────
            Routing.RegisterRoute("Profile", typeof(ProfilePage));

            // ── Super Admin — Form pages ──────────────────────────────────────
            Routing.RegisterRoute("AdminConfigForm", typeof(AdminConfigFormPage));
            Routing.RegisterRoute("AdminCoordinatorForm", typeof(AdminCoordinatorFormPage));
            Routing.RegisterRoute("AdminRouteForm", typeof(AdminRouteFormPage));
            Routing.RegisterRoute("AdminBusForm", typeof(AdminBusFormPage));
            Routing.RegisterRoute("AdminDriverForm", typeof(AdminDriverFormPage));
            Routing.RegisterRoute("AdminParentForm", typeof(AdminParentFormPage));
            Routing.RegisterRoute("AdminStudentForm", typeof(AdminStudentFormPage));
            Routing.RegisterRoute("AdminTripForm", typeof(AdminTripFormPage));

            // ── Super Admin — Detail pages ────────────────────────────────────
            Routing.RegisterRoute("AdminBusDetail", typeof(AdminBusDetailPage));
            Routing.RegisterRoute("AdminCoordinatorDetail", typeof(AdminCoordinatorDetailPage));
            Routing.RegisterRoute("AdminStudentDetail", typeof(AdminStudentDetailPage));
            Routing.RegisterRoute("AdminDriverDetail", typeof(AdminDriverDetailPage));
            Routing.RegisterRoute("AdminRouteDetail", typeof(AdminRouteDetailPage));
            Routing.RegisterRoute("AdminTripDetail", typeof(AdminTripDetailPage));
            Routing.RegisterRoute("AdminParentDetail", typeof(AdminParentDetailPage));
            Routing.RegisterRoute("AdminNotificationDetail", typeof(AdminNotificationDetailPage));
            Routing.RegisterRoute("AdminFeedbackDetail", typeof(AdminFeedbackDetailPage));
            Routing.RegisterRoute("LiveTracking", typeof(LiveTrackingPage));

            // ── Coordinator — Form pages ──────────────────────────────────────
            Routing.RegisterRoute("CoordBusForm", typeof(CoordBusFormPage));
            Routing.RegisterRoute("CoordDriverForm", typeof(CoordDriverFormPage));
            Routing.RegisterRoute("CoordRouteForm", typeof(CoordRouteFormPage));
            Routing.RegisterRoute("CoordStudentForm", typeof(CoordStudentFormPage));
            Routing.RegisterRoute("CoordTripForm", typeof(CoordTripFormPage));
            Routing.RegisterRoute("CoordSubAdminForm", typeof(CoordSubAdminFormPage));
            Routing.RegisterRoute("CoordConfigForm", typeof(CoordConfigFormPage));
            Routing.RegisterRoute("CoordFeedbackDetail", typeof(CoordFeedbackDetailPage));
            Routing.RegisterRoute("CoordNotificationDetail", typeof(CoordNotificationDetailPage));

            // ── Coordinator — Detail pages ────────────────────────────────────
            Routing.RegisterRoute("CoordBusDetail", typeof(CoordBusDetailPage));
            Routing.RegisterRoute("CoordDriverDetail", typeof(CoordDriverDetailPage));
            Routing.RegisterRoute("CoordParentDetail", typeof(CoordParentDetailPage));
            Routing.RegisterRoute("CoordParentForm", typeof(CoordParentFormPage));
            Routing.RegisterRoute("CoordStudentDetail", typeof(CoordStudentDetailPage));
            Routing.RegisterRoute("CoordRouteDetail", typeof(CoordRouteDetailPage));
            Routing.RegisterRoute("CoordTripDetail", typeof(CoordTripDetailPage));
            Routing.RegisterRoute("CoordSubAdminDetail", typeof(CoordSubAdminDetailPage));

            // ── Driver — Detail page ──────────────────────────────────────────
            Routing.RegisterRoute("DriverTripDetail", typeof(DriverTripDetailPage));
            Routing.RegisterRoute("DriverNotification", typeof(DriverNotificationPage));

            Routing.RegisterRoute("DriverTracking", typeof(DriverTrackingPage));
        }

        // ── Services ──────────────────────────────────────────────────────────
        private static void RegisterServices(IServiceCollection s)
        {
#if ANDROID
            s.AddSingleton<IBackgroundLocationService,
                BusTracking.Mobile.Platforms.Android.BackgroundLocationService>();
#elif IOS
            s.AddSingleton<IBackgroundLocationService,
                BusTracking.Mobile.Platforms.iOS.BackgroundLocationService>();
#endif
            s.AddSingleton<ITrackingHubService, TrackingHubService>();
            s.AddSingleton<AppShell>();
            s.AddSingleton<LocalDatabase>();
            s.AddSingleton<ICacheService, CacheService>();
            s.AddSingleton<IApiService, ApiService>();
            s.AddSingleton<IMobileLogService, MobileLogService>();
            s.AddSingleton<INavigationService, NavigationService>();
            s.AddSingleton<IAuthService, AuthService>();

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
            s.AddTransient<ICoordSubAdminService, CoordSubAdminService>();
            s.AddTransient<ICoordAppConfigService, CoordAppConfigService>();
            s.AddTransient<IDriverTripService, DriverTripService>();
            s.AddTransient<IBusTypeService, BusTypeService>();
        }

        // ── ViewModels ────────────────────────────────────────────────────────
        private static void RegisterViewModels(IServiceCollection s)
        {
            // Auth
            s.AddTransient<LoginViewModel>();
            s.AddTransient<ChangePasswordViewModel>();
            s.AddTransient<ProfileViewModel>();

            // SuperAdmin
            s.AddTransient<AdminDashboardViewModel>();
            s.AddTransient<AdminBusListViewModel>();
            s.AddTransient<AdminBusFormViewModel>();
            s.AddTransient<AdminBusDetailViewModel>();
            s.AddTransient<AdminDriverListViewModel>();
            s.AddTransient<AdminDriverFormViewModel>();
            s.AddTransient<AdminDriverDetailViewModel>();
            s.AddTransient<AdminStudentListViewModel>();
            s.AddTransient<AdminStudentFormViewModel>();
            s.AddTransient<AdminStudentDetailViewModel>();
            s.AddTransient<AdminParentListViewModel>();
            s.AddTransient<AdminParentFormViewModel>();
            s.AddTransient<AdminParentDetailViewModel>();
            s.AddTransient<AdminCoordinatorListViewModel>();
            s.AddTransient<AdminCoordinatorFormViewModel>();
            s.AddTransient<AdminCoordinatorDetailViewModel>();
            s.AddTransient<AdminTripListViewModel>();
            s.AddTransient<AdminTripFormViewModel>();
            s.AddTransient<AdminTripDetailViewModel>();
            s.AddTransient<AdminConfigListViewModel>();
            s.AddTransient<AdminConfigFormViewModel>();
            s.AddTransient<AdminRouteListViewModel>();
            s.AddTransient<AdminRouteFormViewModel>();
            s.AddTransient<AdminRouteDetailViewModel>();
            s.AddTransient<AdminNotificationListViewModel>();
            s.AddTransient<AdminNotificationDetailViewModel>();
            s.AddTransient<AdminFeedbackListViewModel>();
            s.AddTransient<AdminFeedbackDetailViewModel>();

            // Coordinator
            s.AddTransient<CoordinatorDashboardViewModel>();
            s.AddTransient<CoordBusListViewModel>();
            s.AddTransient<CoordBusFormViewModel>();
            s.AddTransient<CoordBusDetailViewModel>();
            s.AddTransient<CoordDriverListViewModel>();
            s.AddTransient<CoordDriverFormViewModel>();
            s.AddTransient<CoordDriverDetailViewModel>();
            s.AddTransient<CoordParentListViewModel>();
            s.AddTransient<CoordParentDetailViewModel>();
            s.AddTransient<CoordParentFormViewModel>();
            s.AddTransient<CoordStudentListViewModel>();
            s.AddTransient<CoordStudentFormViewModel>();
            s.AddTransient<CoordStudentDetailViewModel>();
            s.AddTransient<CoordRouteListViewModel>();
            s.AddTransient<CoordRouteFormViewModel>();
            s.AddTransient<CoordRouteDetailViewModel>();
            s.AddTransient<CoordTripListViewModel>();
            s.AddTransient<CoordTripFormViewModel>();
            s.AddTransient<CoordTripDetailViewModel>();
            s.AddTransient<CoordSubAdminListViewModel>();
            s.AddTransient<CoordSubAdminFormViewModel>();
            s.AddTransient<CoordSubAdminDetailViewModel>();
            s.AddTransient<CoordConfigListViewModel>();
            s.AddTransient<CoordConfigFormViewModel>();
            s.AddTransient<CoordFeedbackListViewModel>();
            s.AddTransient<CoordFeedbackDetailViewModel>();
            s.AddTransient<CoordNotificationListViewModel>();
            s.AddTransient<CoordNotificationDetailViewModel>();

            // Driver
            s.AddTransient<DriverDashboardViewModel>();
            s.AddTransient<DriverTripListViewModel>();
            s.AddTransient<DriverTripDetailViewModel>();
            s.AddTransient<DriverTrackingViewModel>();
            s.AddTransient<DriverNotificationViewModel>();

            // Parent
            s.AddTransient<ParentDashboardViewModel>();
            s.AddTransient<ParentTrackingViewModel>();
            s.AddTransient<ParentAvailabilityViewModel>();
            s.AddTransient<ParentFeedbackViewModel>();

            // Student
            s.AddTransient<StudentDashboardViewModel>();
            s.AddTransient<StudentTrackingViewModel>();
            s.AddTransient<StudentAvailabilityViewModel>();

            s.AddTransient<LiveTrackingViewModel>();

            s.AddTransient<AdminBusTypeListViewModel>();
            s.AddTransient<CoordBusTypeListViewModel>();
        }

        // ── Views (Pages) ─────────────────────────────────────────────────────
        private static void RegisterViews(IServiceCollection s)
        {
            // Auth
            s.AddTransient<LoginPage>();
            s.AddTransient<ChangePasswordPage>();

            // Common
            s.AddTransient<MaintenancePage>();
            s.AddTransient<ProfilePage>();

            // SuperAdmin
            s.AddTransient<AdminDashboardPage>();
            s.AddTransient<AdminBusListPage>();
            s.AddTransient<AdminBusFormPage>();
            s.AddTransient<AdminBusDetailPage>();
            s.AddTransient<AdminDriverListPage>();
            s.AddTransient<AdminDriverFormPage>();
            s.AddTransient<AdminDriverDetailPage>();
            s.AddTransient<AdminStudentListPage>();
            s.AddTransient<AdminStudentFormPage>();
            s.AddTransient<AdminStudentDetailPage>();
            s.AddTransient<AdminParentListPage>();
            s.AddTransient<AdminParentFormPage>();
            s.AddTransient<AdminParentDetailPage>();
            s.AddTransient<AdminCoordinatorListPage>();
            s.AddTransient<AdminCoordinatorFormPage>();
            s.AddTransient<AdminCoordinatorDetailPage>();
            s.AddTransient<AdminTripListPage>();
            s.AddTransient<AdminTripFormPage>();
            s.AddTransient<AdminTripDetailPage>();
            s.AddTransient<AdminConfigListPage>();
            s.AddTransient<AdminConfigFormPage>();
            s.AddTransient<AdminRouteListPage>();
            s.AddTransient<AdminRouteFormPage>();
            s.AddTransient<AdminRouteDetailPage>();
            s.AddTransient<AdminNotificationListPage>();
            s.AddTransient<AdminNotificationDetailPage>();
            s.AddTransient<AdminFeedbackListPage>();
            s.AddTransient<AdminFeedbackDetailPage>();

            // Coordinator
            s.AddTransient<CoordinatorDashboardPage>();
            s.AddTransient<CoordBusListPage>();
            s.AddTransient<CoordBusFormPage>();
            s.AddTransient<CoordBusDetailPage>();
            s.AddTransient<CoordDriverListPage>();
            s.AddTransient<CoordDriverFormPage>();
            s.AddTransient<CoordDriverDetailPage>();
            s.AddTransient<CoordParentListPage>();
            s.AddTransient<CoordParentDetailPage>();
            s.AddTransient<CoordParentFormPage>();
            s.AddTransient<CoordStudentListPage>();
            s.AddTransient<CoordStudentFormPage>();
            s.AddTransient<CoordStudentDetailPage>();
            s.AddTransient<CoordRouteListPage>();
            s.AddTransient<CoordRouteFormPage>();
            s.AddTransient<CoordRouteDetailPage>();
            s.AddTransient<CoordTripListPage>();
            s.AddTransient<CoordTripFormPage>();
            s.AddTransient<CoordTripDetailPage>();
            s.AddTransient<CoordSubAdminListPage>();
            s.AddTransient<CoordSubAdminFormPage>();
            s.AddTransient<CoordSubAdminDetailPage>();
            s.AddTransient<CoordConfigListPage>();
            s.AddTransient<CoordConfigFormPage>();
            s.AddTransient<CoordFeedbackListPage>();
            s.AddTransient<CoordFeedbackDetailPage>();
            s.AddTransient<CoordNotificationListPage>();
            s.AddTransient<CoordNotificationDetailPage>();

            // Driver
            s.AddTransient<DriverDashboardPage>();
            s.AddTransient<DriverTripListPage>();
            s.AddTransient<DriverTripDetailPage>();
            s.AddTransient<DriverTrackingPage>();
            s.AddTransient<DriverNotificationPage>();

            // Parent
            s.AddTransient<ParentDashboardPage>();
            s.AddTransient<ParentTrackingPage>();
            s.AddTransient<ParentAvailabilityPage>();
            s.AddTransient<ParentFeedbackPage>();

            // Student
            s.AddTransient<StudentDashboardPage>();
            s.AddTransient<StudentTrackingPage>();
            s.AddTransient<StudentAvailabilityPage>();

            s.AddTransient<LiveTrackingPage>();

            s.AddTransient<AdminBusTypeListPage>();
            s.AddTransient<CoordBusTypeListPage>();
        }

        // ── Control Handlers ──────────────────────────────────────────────────
        static void MauiControlsHandlers()
        {
            #region DatePicker Handler
            Microsoft.Maui.Handlers.DatePickerHandler.Mapper.AppendToMapping("MyDatePickerHandler", (handler, view) =>
            {
#if ANDROID
                handler.PlatformView.Background = null;
                handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
                handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#endif
#if IOS
                handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
                handler.PlatformView.Layer.BorderWidth = 0;
                handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#endif
            });
            #endregion

            #region Entry
            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("MyBorderlessEntryHandler", (handler, view) =>
            {
                if (view is Entry)
                {
#if ANDROID
                    handler.PlatformView.Background = null;
                    handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
                    handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#elif IOS
                    handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
                    handler.PlatformView.Layer.BorderWidth = 0;
                    handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#endif
                }
            });
            #endregion

            #region Picker Handler
            Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("MyPickerHandler", (handler, view) =>
            {
                if (view is Picker picker)
                {
#if ANDROID
                    picker.SelectedIndexChanged += (s, e) =>
                    {
                        if (picker.IsFocused) picker.Unfocus();
                    };
                    handler.PlatformView.Background = null;
                    handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
                    handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#elif IOS
                    handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
                    handler.PlatformView.Layer.BorderWidth = 0;
                    handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#endif
                }
            });
            #endregion

            #region Editor Handler
            Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping("MyEditorHandler", (handler, view) =>
            {
#if ANDROID
                handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#endif
            });
            #endregion

            #region TimePicker Handler
            Microsoft.Maui.Handlers.TimePickerHandler.Mapper.AppendToMapping("MyTimePickerHandler", (handler, view) =>
            {
#if ANDROID
                handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#elif IOS
                handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
                handler.PlatformView.Layer.BorderWidth = 0;
                handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#endif
            });
            #endregion
        }
    }
}