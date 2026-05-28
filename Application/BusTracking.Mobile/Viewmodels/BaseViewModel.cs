namespace BusTracking.Mobile.ViewModels;

/// <summary>
/// Base ViewModel — all ViewModels inherit from this.
/// Provides: IsBusy, Title, Error handling, Navigation helpers.
/// Zero code in .xaml.cs — everything binds through here.
/// </summary>
public abstract partial class BaseViewModel : ObservableObject
{
    protected readonly IAuthService Auth;
    protected readonly INavigationService Nav;

    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string _title = "";
    [ObservableProperty] private string _errorMessage = "";
    [ObservableProperty] private bool _hasError;
    [ObservableProperty] private bool _isEmpty;

    protected BaseViewModel(IAuthService auth, INavigationService nav)
    {
        Auth = auth;
        Nav = nav;
    }

    /// <summary>Run an async task with busy indicator and error handling.</summary>
    protected async Task RunAsync(Func<Task> work, string? busyMessage = null)
    {
        if (IsBusy) return;
        IsBusy = true;
        HasError = false;
        ErrorMessage = "";
        try
        {
            await work();
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected void SetError(string message)
    {
        ErrorMessage = message;
        HasError = true;
    }

    protected void ClearError()
    {
        ErrorMessage = "";
        HasError = false;
    }

    protected async Task ShowAlertAsync(string title, string message, string cancel = "OK")
    {
        if (Application.Current?.Windows[0].Page is Page p)
            await p.DisplayAlertAsync(title, message, cancel);
    }

    protected async Task<bool> ConfirmAsync(string title, string message,
        string accept = "Yes", string cancel = "No")
    {
        if (Application.Current?.Windows[0].Page is Page p)
            return await p.DisplayAlertAsync(title, message, accept, cancel);
        return false;
    }

    protected async Task ShowToastAsync(string message)
    {
        // Requires CommunityToolkit.Maui — falls back to DisplayAlert if not available
        try
        {
            var toast = CommunityToolkit.Maui.Alerts.Toast.Make(message);
            await toast.Show();
        }
        catch
        {
            await ShowAlertAsync("Info", message);
        }
    }

    /// <summary>Check if user has permission. SuperAdmin always returns true.</summary>
    protected bool Can(string permissionKey) => Auth.HasPermission(permissionKey);
}

// Extension of BaseViewModel with lifecycle support
public abstract partial class BaseViewModel
{
    public virtual Task InitializeAsync() => Task.CompletedTask;
}