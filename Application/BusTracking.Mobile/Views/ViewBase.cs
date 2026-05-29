namespace BusTracking.Mobile.Views;

public abstract class ViewBase<TViewModel> : ContentPage
    where TViewModel : BaseViewModel
{
    protected TViewModel ViewModel { get; }
    private bool _initialized;

    protected ViewBase(TViewModel viewModel)
    {
        ViewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_initialized)
        {
            // First visit — full init
            _initialized = true;
            await ViewModel.InitializeAsync();
        }
        else
        {
            // Returning from a child page (e.g. after Add/Edit form) — refresh list
            await ViewModel.RefreshOnReturnAsync();
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    protected static Color GetThemeColor(string lightKey, string darkKey)
    {
        var theme = Application.Current?.RequestedTheme ?? AppTheme.Light;
        var key = theme == AppTheme.Dark ? darkKey : lightKey;
        if (Application.Current?.Resources.TryGetValue(key, out var raw) == true && raw is Color c)
            return c;
        return Colors.Transparent;
    }

#if ANDROID
    private static Android.Graphics.Color ToAndroidColor(Color c) =>
        Android.Graphics.Color.Argb(
            (int)(c.Alpha * 255),
            (int)(c.Red * 255),
            (int)(c.Green * 255),
            (int)(c.Blue * 255));
#endif

    protected static void SetSystemBarsColor(
        string lightStatusKey,
        string darkStatusKey,
        string lightNavBarKey,
        string darkNavBarKey,
        bool useLightStatusIcons = true,
        bool useLightNavIcons = false)
    {
#if ANDROID
#pragma warning disable CA1422
        var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
        if (activity?.Window is null) return;

        var statusColor = ToAndroidColor(GetThemeColor(lightStatusKey, darkStatusKey));
        var navColor = ToAndroidColor(GetThemeColor(lightNavBarKey, darkNavBarKey));

        activity.Window.SetStatusBarColor(statusColor);
        activity.Window.SetNavigationBarColor(navColor);

        if (OperatingSystem.IsAndroidVersionAtLeast(30))
        {
            var insetsController = activity.Window.InsetsController;
            if (insetsController != null)
            {
                if (useLightStatusIcons)
                    insetsController.SetSystemBarsAppearance(0,
                        (int)Android.Views.WindowInsetsControllerAppearance.LightStatusBars);
                else
                    insetsController.SetSystemBarsAppearance(
                        (int)Android.Views.WindowInsetsControllerAppearance.LightStatusBars,
                        (int)Android.Views.WindowInsetsControllerAppearance.LightStatusBars);

                if (useLightNavIcons)
                    insetsController.SetSystemBarsAppearance(0,
                        (int)Android.Views.WindowInsetsControllerAppearance.LightNavigationBars);
                else
                    insetsController.SetSystemBarsAppearance(
                        (int)Android.Views.WindowInsetsControllerAppearance.LightNavigationBars,
                        (int)Android.Views.WindowInsetsControllerAppearance.LightNavigationBars);
            }
        }
        else if (OperatingSystem.IsAndroidVersionAtLeast(26))
        {
            var flags = activity.Window.DecorView.SystemUiFlags;
            flags = useLightStatusIcons
                ? flags & ~Android.Views.SystemUiFlags.LightStatusBar
                : flags | Android.Views.SystemUiFlags.LightStatusBar;
            flags = useLightNavIcons
                ? flags & ~Android.Views.SystemUiFlags.LightNavigationBar
                : flags | Android.Views.SystemUiFlags.LightNavigationBar;
            activity.Window.DecorView.SystemUiFlags = flags;
        }
#pragma warning restore CA1422
#endif
    }
}