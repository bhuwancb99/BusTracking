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
            RegisterRoutes();           // ← MUST be called before Build()
            RegisterServices(builder.Services);
            RegisterViewModels(builder.Services);
            RegisterViews(builder.Services);

            return builder.Build();
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

            // ── Super Admin — List pages (pushed from Dashboard icon taps) ────
            Routing.RegisterRoute("AdminDashboard", typeof(AdminDashboardPage));
            Routing.RegisterRoute("AdminConfigList", typeof(AdminConfigListPage));
            Routing.RegisterRoute("AdminCoordinatorList", typeof(AdminCoordinatorListPage));
            Routing.RegisterRoute("AdminBusList", typeof(AdminBusListPage));
            Routing.RegisterRoute("AdminDriverList", typeof(AdminDriverListPage));
            Routing.RegisterRoute("AdminParentList", typeof(AdminParentListPage));
            Routing.RegisterRoute("AdminStudentList", typeof(AdminStudentListPage));
            Routing.RegisterRoute("AdminTripList", typeof(AdminTripListPage));

            // ── Super Admin — Form pages (pushed from list Add/Edit buttons) ──
            Routing.RegisterRoute("AdminConfigForm", typeof(AdminConfigFormPage));
            Routing.RegisterRoute("AdminCoordinatorForm", typeof(AdminCoordinatorFormPage));
            Routing.RegisterRoute("AdminRouteForm", typeof(AdminRouteFormPage));
            Routing.RegisterRoute("AdminBusForm", typeof(AdminBusFormPage));
            Routing.RegisterRoute("AdminDriverForm", typeof(AdminDriverFormPage));
            Routing.RegisterRoute("AdminParentForm", typeof(AdminParentFormPage));
            Routing.RegisterRoute("AdminStudentForm", typeof(AdminStudentFormPage));
            Routing.RegisterRoute("AdminTripForm", typeof(AdminTripFormPage));

            // ── Super Admin — Detail pages (pushed from list View buttons) ────
            Routing.RegisterRoute("AdminBusDetail", typeof(AdminBusDetailPage));
            Routing.RegisterRoute("AdminCoordinatorDetail", typeof(AdminCoordinatorDetailPage));
            Routing.RegisterRoute("AdminRouteDetail", typeof(AdminRouteDetailPage));
            Routing.RegisterRoute("AdminTripDetail", typeof(AdminTripDetailPage));

            // ── Coordinator — List pages (pushed from dashboard icon taps) ────
            Routing.RegisterRoute("CoordinatorDashboard", typeof(CoordinatorDashboardPage));
            Routing.RegisterRoute("CoordRouteList", typeof(CoordRouteListPage));
            Routing.RegisterRoute("CoordBusList", typeof(CoordBusListPage));
            Routing.RegisterRoute("CoordDriverList", typeof(CoordDriverListPage));
            Routing.RegisterRoute("CoordParentList", typeof(CoordParentListPage));
            Routing.RegisterRoute("CoordStudentList", typeof(CoordStudentListPage));
            Routing.RegisterRoute("CoordTripList", typeof(CoordTripListPage));

            // ── Coordinator — Form pages ───────────────────────────────────────
            Routing.RegisterRoute("CoordBusForm", typeof(CoordBusFormPage));
            Routing.RegisterRoute("CoordRouteForm", typeof(CoordRouteFormPage));
            Routing.RegisterRoute("CoordStudentForm", typeof(CoordStudentFormPage));
            Routing.RegisterRoute("CoordTripForm", typeof(CoordTripFormPage));

            // ── Coordinator — Detail pages ────────────────────────────────────
            Routing.RegisterRoute("CoordBusDetail", typeof(CoordBusDetailPage));
            Routing.RegisterRoute("CoordDriverDetail", typeof(CoordDriverDetailPage));
            Routing.RegisterRoute("CoordParentDetail", typeof(CoordParentDetailPage));
            Routing.RegisterRoute("CoordStudentDetail", typeof(CoordStudentDetailPage));
            Routing.RegisterRoute("CoordRouteDetail", typeof(CoordRouteDetailPage));
            Routing.RegisterRoute("CoordTripDetail", typeof(CoordTripDetailPage));

            // ── Driver ────────────────────────────────────────────────────────
            Routing.RegisterRoute("DriverDashboard", typeof(DriverDashboardPage));
            Routing.RegisterRoute("DriverTripList", typeof(DriverTripListPage));
            Routing.RegisterRoute("DriverTripDetail", typeof(DriverTripDetailPage));
            Routing.RegisterRoute("DriverTracking", typeof(DriverTrackingPage));

            // ── Parent ────────────────────────────────────────────────────────
            Routing.RegisterRoute("ParentDashboard", typeof(ParentDashboardPage));
            Routing.RegisterRoute("ParentTracking", typeof(ParentTrackingPage));

            // ── Student ───────────────────────────────────────────────────────
            Routing.RegisterRoute("StudentDashboard", typeof(StudentDashboardPage));
            Routing.RegisterRoute("StudentTracking", typeof(StudentTrackingPage));
            Routing.RegisterRoute("StudentAvailability", typeof(StudentAvailabilityPage));
        }

        // ── Services ──────────────────────────────────────────────────────────
        private static void RegisterServices(IServiceCollection s)
        {
            s.AddSingleton<AppShell>();
            s.AddSingleton<LocalDatabase>();
            s.AddSingleton<ICacheService, CacheService>();
            s.AddSingleton<IApiService, ApiService>();
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
            s.AddTransient<AdminBusDetailViewModel>();
            s.AddTransient<AdminDriverListViewModel>();
            s.AddTransient<AdminDriverFormViewModel>();
            s.AddTransient<AdminStudentListViewModel>();
            s.AddTransient<AdminStudentFormViewModel>();
            s.AddTransient<AdminParentListViewModel>();
            s.AddTransient<AdminParentFormViewModel>();
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

            // Coordinator
            s.AddTransient<CoordinatorDashboardViewModel>();
            s.AddTransient<CoordBusListViewModel>();
            s.AddTransient<CoordBusFormViewModel>();
            s.AddTransient<CoordBusDetailViewModel>();
            s.AddTransient<CoordDriverListViewModel>();
            s.AddTransient<CoordDriverDetailViewModel>();
            s.AddTransient<CoordParentListViewModel>();
            s.AddTransient<CoordParentDetailViewModel>();
            s.AddTransient<CoordStudentListViewModel>();
            s.AddTransient<CoordStudentFormViewModel>();
            s.AddTransient<CoordStudentDetailViewModel>();
            s.AddTransient<CoordRouteListViewModel>();
            s.AddTransient<CoordRouteFormViewModel>();
            s.AddTransient<CoordRouteDetailViewModel>();
            s.AddTransient<CoordTripListViewModel>();
            s.AddTransient<CoordTripFormViewModel>();
            s.AddTransient<CoordTripDetailViewModel>();

            // Driver
            s.AddTransient<DriverDashboardViewModel>();
            s.AddTransient<DriverTripListViewModel>();
            s.AddTransient<DriverTripDetailViewModel>();
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
        // Pages must be registered in DI so the ServiceProvider can resolve them
        // when Routing.RegisterRoute creates instances via the DI container.
        private static void RegisterViews(IServiceCollection s)
        {
            // Auth
            s.AddTransient<LoginPage>();
            s.AddTransient<ChangePasswordPage>();

            // SuperAdmin
            s.AddTransient<AdminDashboardPage>();
            s.AddTransient<AdminBusListPage>();
            s.AddTransient<AdminBusFormPage>();
            s.AddTransient<AdminBusDetailPage>();
            s.AddTransient<AdminDriverListPage>();
            s.AddTransient<AdminDriverFormPage>();
            s.AddTransient<AdminStudentListPage>();
            s.AddTransient<AdminStudentFormPage>();
            s.AddTransient<AdminParentListPage>();
            s.AddTransient<AdminParentFormPage>();
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

            // Coordinator
            s.AddTransient<CoordinatorDashboardPage>();
            s.AddTransient<CoordBusListPage>();
            s.AddTransient<CoordBusFormPage>();
            s.AddTransient<CoordBusDetailPage>();
            s.AddTransient<CoordDriverListPage>();
            s.AddTransient<CoordDriverDetailPage>();
            s.AddTransient<CoordParentListPage>();
            s.AddTransient<CoordParentDetailPage>();
            s.AddTransient<CoordStudentListPage>();
            s.AddTransient<CoordStudentFormPage>();
            s.AddTransient<CoordStudentDetailPage>();
            s.AddTransient<CoordRouteListPage>();
            s.AddTransient<CoordRouteFormPage>();
            s.AddTransient<CoordRouteDetailPage>();
            s.AddTransient<CoordTripListPage>();
            s.AddTransient<CoordTripFormPage>();
            s.AddTransient<CoordTripDetailPage>();

            // Driver
            s.AddTransient<DriverDashboardPage>();
            s.AddTransient<DriverTripListPage>();
            s.AddTransient<DriverTripDetailPage>();
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