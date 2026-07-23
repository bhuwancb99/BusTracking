namespace BusTracking.Mobile.ViewModels;

/// <summary>
/// Base ViewModel — all ViewModels inherit from this.
/// Provides: IsBusy, Title, Error handling, Navigation helpers.
/// </summary>
public abstract partial class BaseViewModel : ObservableObject
{
    protected readonly IAuthService Auth;
    protected readonly INavigationService Nav;

    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isRefreshing;
    [ObservableProperty] private string _title = "";
    [ObservableProperty] private string _errorMessage = "";
    [ObservableProperty] private bool _hasError;
    [ObservableProperty] private bool _isEmpty;

    protected BaseViewModel(IAuthService auth, INavigationService nav)
    {
        Auth = auth;
        Nav = nav;
    }

    /// <summary>First-time load. Override in every ViewModel.</summary>
    public virtual Task InitializeAsync() => Task.CompletedTask;

    /// <summary>
    /// Called every time a page re-appears (i.e. navigating back from a child page).
    /// List ViewModels override this to reload their data so Add/Edit changes are visible.
    /// Form ViewModels leave this as no-op (they will be popped off the stack).
    /// </summary>
    public virtual Task RefreshOnReturnAsync() => Task.CompletedTask;

    /// <summary>Run an async task with busy indicator and error handling.</summary>
    protected async Task RunAsync(Func<Task> work, string? busyMessage = null)
    {
        if (IsBusy) return;
        IsBusy = true;
        HasError = false;
        ErrorMessage = "";
        try { await work(); }
        catch (Exception ex)
        {
            SetError(ex.Message);
            try
            {
                var logService = Microsoft.Maui.IPlatformApplication.Current?.Services.GetService<BusTracking.Mobile.Interfaces.IMobileLogService>();
                if (logService != null)
                {
                    await logService.LogExceptionAsync(ex, Title, "RunAsync");
                }
            }
            catch { }
        }
        finally { IsBusy = false; }
    }

    protected void SetError(string message) { ErrorMessage = message; HasError = true; }
    protected void ClearError() { ErrorMessage = ""; HasError = false; }

    protected async Task ShowAlertAsync(string title, string message, string cancel = "OK")
    {
        if (Application.Current?.Windows[0].Page is Page p)
            await p.DisplayAlertAsync(title, message, cancel);
    }

    protected async Task<bool> ConfirmAsync(string title, string message,
        string accept = "Yes", string cancel = "No")
    {
        if (Application.Current?.Windows[0].Page is Page p)
        {
            string iconSource = "info.png";
            Color? acceptColor = null;

            if (title.Contains("Logout", StringComparison.OrdinalIgnoreCase))
            {
                iconSource = "logout.png";
                acceptColor = Color.FromArgb("#ba1a1a");
                if (accept == "Yes") accept = "Yes, Logout";
                if (cancel == "No") cancel = "Cancel";
            }
            else if (title.Contains("Delete", StringComparison.OrdinalIgnoreCase) || title.Contains("Remove", StringComparison.OrdinalIgnoreCase))
            {
                iconSource = "delete.png";
                acceptColor = Color.FromArgb("#ba1a1a");
            }
            else
            {
                acceptColor = Color.FromArgb("#512BD4"); // Primary
            }

            var popup = new Views.Common.ConfirmPopup(title, message, accept, cancel, iconSource, acceptColor);
            var result = await p.ShowPopupAsync<bool>(popup);
            return result is not null && result.Result;
        }
        return false;
    }

    protected async Task ShowToastAsync(string message)
    {
        try
        {
            var toast = CommunityToolkit.Maui.Alerts.Toast.Make(message);
            await toast.Show();
        }
        catch { await ShowAlertAsync("Info", message); }
    }

    /// <summary>Check if user has permission. SuperAdmin always returns true.</summary>
    protected bool Can(string permissionKey) => Auth.HasPermission(permissionKey);

    [ObservableProperty] private bool _isNotificationPermissionDenied;

    [RelayCommand]
    public void OpenNotificationSettings()
    {
        var notifService = Microsoft.Maui.IPlatformApplication.Current?.Services.GetService<BusTracking.Mobile.Interfaces.INotificationPermissionService>();
        notifService?.OpenAppSettings();
    }

    public async Task CheckNotificationPermissionAsync(bool requestIfFirstTime = false)
    {
        try
        {
            var notifService = Microsoft.Maui.IPlatformApplication.Current?.Services.GetService<BusTracking.Mobile.Interfaces.INotificationPermissionService>();
            if (notifService == null) return;

            if (requestIfFirstTime)
            {
                var granted = await notifService.RequestNotificationPermissionAsync();
                IsNotificationPermissionDenied = !granted;
                if (granted) _ = RegisterPushTokenAsync();
            }
            else
            {
                var granted = await notifService.IsNotificationPermissionGrantedAsync();
                IsNotificationPermissionDenied = !granted;
                if (granted) _ = RegisterPushTokenAsync();
            }
        }
        catch
        {
            IsNotificationPermissionDenied = false;
        }
    }

    public async Task RegisterPushTokenAsync()
    {
        try
        {
            var tokenService = Microsoft.Maui.IPlatformApplication.Current?.Services.GetService<BusTracking.Mobile.Interfaces.IPushTokenService>();
            if (tokenService != null)
            {
                await tokenService.RegisterDeviceTokenAsync();
            }
        }
        catch { }
    }
}