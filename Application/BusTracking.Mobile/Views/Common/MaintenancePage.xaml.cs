namespace BusTracking.Mobile.Views.Common;

/// <summary>
/// Startup gate page — first page the app loads.
/// Checks GlobalConfig for maintenance mode and app version updates BEFORE showing login or dashboard.
/// Flow:
///   1. Show loading spinner ("Checking system status...")
///   2. Fetch GlobalConfigurations from API
///   3a. If maintenance → show maintenance UI (full screen, no escape)
///   3b. If update needed → show UpdatePopup (mandatory or optional)
///   3c. If authenticated → navigate directly to user's role Dashboard
///   3d. Otherwise → navigate to //Login
/// </summary>
public partial class MaintenancePage : ContentPage
{
    private readonly IGlobalConfigService _globalConfig;
    private readonly IAuthService _auth;
    private readonly INavigationService _nav;
    private bool _checked; // guard against OnAppearing running check twice

    public MaintenancePage(IGlobalConfigService globalConfig, IAuthService auth, INavigationService nav)
    {
        _globalConfig = globalConfig;
        _auth = auth;
        _nav = nav;
        InitializeComponent();
    }

    /// <summary>
    /// Sets the maintenance message text.
    /// </summary>
    public void SetMessage(string message)
    {
        if (!string.IsNullOrWhiteSpace(message))
            MessageLabel.Text = message;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Ensure user cannot navigate back
        Shell.SetNavBarIsVisible(this, false);
        Shell.SetTabBarIsVisible(this, false);
        Shell.SetFlyoutBehavior(this, FlyoutBehavior.Disabled);

        if (!_checked)
        {
            _checked = true;
            await CheckSystemStatusAsync();
        }
    }

    /// <summary>
    /// Main startup check — runs once on first appearance.
    /// </summary>
    private async Task CheckSystemStatusAsync()
    {
        try
        {
            // Show loading state
            LoadingPanel.IsVisible = true;
            MaintenancePanel.IsVisible = false;

            var cfg = await _globalConfig.GetGlobalConfigAsync(forceRefresh: true);

            // ── 1. Maintenance check ────────────────────────────────
            if (await _globalConfig.IsMaintenanceModeAsync())
            {
                string message = cfg.GetValueOrDefault("MaintenanceMessage",
                    "We are under maintenance. Please check back soon.");
                ShowMaintenanceUI(message);
                return;
            }

            // ── 2. Version update check ─────────────────────────────
            await CheckAppVersionAndUpdateAsync(cfg);

            // ── 3. All clear → check authentication & navigate ──────
            if (await NavigateIfAuthenticatedAsync())
                return;

            // Fallback to login if not authenticated
            await Shell.Current.GoToAsync("//Login", false);
        }
        catch
        {
            // API unreachable — try offline session restore or fallback to login
            if (!await NavigateIfAuthenticatedAsync())
            {
                await Shell.Current.GoToAsync("//Login", false);
            }
        }
    }

    /// <summary>
    /// Checks if user session is active and navigates directly to role dashboard.
    /// Returns true if navigation occurred, false if not authenticated.
    /// </summary>
    private async Task<bool> NavigateIfAuthenticatedAsync()
    {
        try
        {
            if (await _auth.IsAuthenticatedAsync())
            {
                var user = await _auth.GetCurrentUserAsync();
                if (user is not null)
                {
                    await _nav.GoToDashboardAsync(user.Role);
                    return true;
                }
            }
        }
        catch
        {
            // Session corrupt or cleared
        }
        return false;
    }

    /// <summary>
    /// Switches from loading spinner to full maintenance UI.
    /// </summary>
    private void ShowMaintenanceUI(string message)
    {
        LoadingPanel.IsVisible = false;
        MaintenancePanel.IsVisible = true;
        MessageLabel.Text = message;
    }

    /// <summary>
    /// Retry button handler — re-checks maintenance mode.
    /// If maintenance is off, check authentication and navigate to dashboard or login.
    /// </summary>
    private async void OnRetryClicked(object? sender, EventArgs e)
    {
        RetryButton.IsEnabled = false;
        RetryButton.Text = "Checking...";

        try
        {
            var cfg = await _globalConfig.GetGlobalConfigAsync(forceRefresh: true);

            if (!await _globalConfig.IsMaintenanceModeAsync())
            {
                // Maintenance is over — check update then navigate
                await CheckAppVersionAndUpdateAsync(cfg);

                if (!await NavigateIfAuthenticatedAsync())
                {
                    await Shell.Current.GoToAsync("//Login", true);
                }
                return;
            }

            // Still under maintenance — show styled popup
            await ShowStyledAlertAsync("Still Under Maintenance",
                "The system is still under maintenance. Please try again later.",
                "config.png", Color.FromArgb("#ffa929"));
        }
        catch
        {
            await ShowStyledAlertAsync("Connection Error",
                "Unable to reach the server. Please check your internet connection and try again.",
                "info.png", Color.FromArgb("#ba1a1a"));
        }
        finally
        {
            RetryButton.Text = "Retry";
            RetryButton.IsEnabled = true;
        }
    }

    /// <summary>
    /// Checks installed app version against GlobalConfig target version.
    /// Shows UpdatePopup if update is needed.
    /// </summary>
    private async Task CheckAppVersionAndUpdateAsync(Dictionary<string, string> cfg)
    {
        try
        {
            string currentVersionStr = AppInfo.Current.VersionString;
            bool isAndroid = DeviceInfo.Current.Platform == DevicePlatform.Android;

            string targetVersionStr = isAndroid
                ? cfg.GetValueOrDefault("AndroidVersion", "1.0")
                : cfg.GetValueOrDefault("iOSVersion", "1.0");

            string updateUrl = isAndroid
                ? cfg.GetValueOrDefault("Android_Update_Url", "")
                : cfg.GetValueOrDefault("iOS_Update_Url", "");

            if (Version.TryParse(currentVersionStr, out var currentVersion) &&
                Version.TryParse(targetVersionStr, out var targetVersion))
            {
                if (currentVersion < targetVersion)
                {
                    bool isMandatory = await _globalConfig.IsMandatoryUpdateAsync();

                    if (Application.Current?.Windows[0].Page is Page page)
                    {
                        while (true)
                        {
                            var popup = new UpdatePopup(
                                currentVersionStr, targetVersionStr, updateUrl, isMandatory);

                            await page.ShowPopupAsync<string>(popup);

                            if (isMandatory)
                            {
                                // Mandatory: popup re-shows after user returns from store
                                continue;
                            }
                            else
                            {
                                // Optional: user tapped "Later" or "Update Now" — break
                                break;
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            // Non-fatal version check failure
        }
    }

    /// <summary>
    /// Shows a themed AlertPopup matching the ConfirmPopup style.
    /// </summary>
    private async Task ShowStyledAlertAsync(string title, string message,
        string iconSource = "info.png", Color? iconColor = null)
    {
        if (Application.Current?.Windows[0].Page is Page p)
        {
            var popup = new AlertPopup(title, message, "OK", iconSource, iconColor);
            await p.ShowPopupAsync(popup);
        }
    }

    /// <summary>
    /// Override back button to prevent navigation away from maintenance screen.
    /// </summary>
    protected override bool OnBackButtonPressed()
    {
        return true;
    }
}