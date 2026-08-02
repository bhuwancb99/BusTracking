namespace BusTracking.Mobile.Views.Common;

public partial class MaintenancePage : ContentPage
{
    private readonly IGlobalConfigService _globalConfig;

    public MaintenancePage(IGlobalConfigService globalConfig)
    {
        _globalConfig = globalConfig;
        InitializeComponent();
    }

    /// <summary>
    /// Sets the maintenance message text from the ViewModel / caller.
    /// </summary>
    public void SetMessage(string message)
    {
        if (!string.IsNullOrWhiteSpace(message))
            MessageLabel.Text = message;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Ensure user cannot navigate back
        Shell.SetNavBarIsVisible(this, false);
        Shell.SetTabBarIsVisible(this, false);
        Shell.SetFlyoutBehavior(this, FlyoutBehavior.Disabled);
    }

    /// <summary>
    /// Retry button handler — re-checks maintenance mode.
    /// If maintenance is off, navigate back to Login.
    /// </summary>
    private async void OnRetryClicked(object? sender, EventArgs e)
    {
        RetryButton.IsEnabled = false;
        RetryButton.Text = "Checking...";

        try
        {
            // Force-refresh config from the server
            await _globalConfig.GetGlobalConfigAsync(forceRefresh: true);

            bool stillMaintenance = await _globalConfig.IsMaintenanceModeAsync();
            if (!stillMaintenance)
            {
                // Maintenance is over — go back to login
                await Shell.Current.GoToAsync("//Login", true);
                return;
            }

            // Still under maintenance
            if (Application.Current?.Windows[0].Page is Page p)
            {
                await p.DisplayAlertAsync("Still Under Maintenance",
                    "The system is still under maintenance. Please try again later.", "OK");
            }
        }
        catch
        {
            if (Application.Current?.Windows[0].Page is Page p)
            {
                await p.DisplayAlertAsync("Connection Error",
                    "Unable to reach the server. Please check your internet connection and try again.", "OK");
            }
        }
        finally
        {
            RetryButton.Text = "Retry";
            RetryButton.IsEnabled = true;
        }
    }

    /// <summary>
    /// Override back button to prevent navigation away from maintenance screen.
    /// </summary>
    protected override bool OnBackButtonPressed()
    {
        // Block hardware back button — user must stay on maintenance page
        return true;
    }
}