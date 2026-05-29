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
                    //fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif
            MauiControlsHandlers();
            RegisterServices(builder.Services);
            RegisterViewModels(builder.Services);
            RegisterViews(builder.Services);

            return builder.Build();
        }

        // ── Services ──────────────────────────────────────────────────────────
        private static void RegisterServices(IServiceCollection s)
        {
            // Infrastructure — Singletons (one instance for app lifetime)
            s.AddSingleton<AppShell>();
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
            s.AddTransient<AdminRouteListViewModel>();
            s.AddTransient<AdminBusDetailViewModel>();
            s.AddTransient<AdminCoordinatorDetailViewModel>();
            s.AddTransient<AdminRouteDetailViewModel>();
            s.AddTransient<AdminRouteFormViewModel>();
            s.AddTransient<AdminStudentFormViewModel>();
            s.AddTransient<AdminTripDetailViewModel>();
            s.AddTransient<AdminTripFormViewModel>();

            // Coordinator
            s.AddTransient<CoordinatorDashboardViewModel>();
            s.AddTransient<CoordBusListViewModel>();
            s.AddTransient<CoordTripListViewModel>();
            s.AddTransient<CoordStudentListViewModel>();
            s.AddTransient<CoordParentListViewModel>();
            s.AddTransient<CoordDriverListViewModel>();
            s.AddTransient<CoordRouteListViewModel>();
            s.AddTransient<CoordBusFormViewModel>();
            s.AddTransient<CoordBusDetailViewModel>();
            s.AddTransient<CoordDriverDetailViewModel>();
            s.AddTransient<CoordParentDetailViewModel>();
            s.AddTransient<CoordStudentFormViewModel>();
            s.AddTransient<CoordStudentDetailViewModel>();
            s.AddTransient<CoordRouteFormViewModel>();
            s.AddTransient<CoordRouteDetailViewModel>();
            s.AddTransient<CoordTripFormViewModel>();
            s.AddTransient<CoordTripDetailViewModel>();

            // Driver
            s.AddTransient<DriverDashboardViewModel>();
            s.AddTransient<DriverTripListViewModel>();
            s.AddTransient<DriverTrackingViewModel>();
            s.AddTransient<DriverTripDetailViewModel>();

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
            s.AddTransient<AdminRouteListPage>();
            s.AddTransient<AdminDriverFormPage>();
            s.AddTransient<AdminParentFormPage>();
            s.AddTransient<AdminRouteFormPage>();
            s.AddTransient<AdminStudentFormPage>();
            s.AddTransient<AdminTripFormPage>();
            s.AddTransient<AdminBusDetailPage>();
            s.AddTransient<AdminCoordinatorDetailPage>();
            s.AddTransient<AdminRouteDetailPage>();
            s.AddTransient<AdminTripDetailPage>();

            // Coordinator
            s.AddTransient<CoordinatorDashboardPage>();
            s.AddTransient<CoordBusListPage>();
            s.AddTransient<CoordTripListPage>();
            s.AddTransient<CoordStudentListPage>();
            s.AddTransient<CoordParentListPage>();
            s.AddTransient<CoordDriverListPage>();
            s.AddTransient<CoordRouteListPage>();
            s.AddTransient<CoordBusFormPage>();
            s.AddTransient<CoordBusDetailPage>();
            s.AddTransient<CoordDriverDetailPage>();
            s.AddTransient<CoordParentDetailPage>();
            s.AddTransient<CoordStudentFormPage>();
            s.AddTransient<CoordStudentDetailPage>();
            s.AddTransient<CoordRouteFormPage>();
            s.AddTransient<CoordRouteDetailPage>();
            s.AddTransient<CoordTripFormPage>();
            s.AddTransient<CoordTripDetailPage>();

            // Driver
            s.AddTransient<DriverDashboardPage>();
            s.AddTransient<DriverTripListPage>();
            s.AddTransient<DriverTrackingPage>();
            s.AddTransient<DriverTripDetailPage>();

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

        /// <summary>
        /// MauiControlsHandlers
        /// </summary>
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
                        if (picker.IsFocused)
                        {
                            picker.Unfocus();
                        }
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
