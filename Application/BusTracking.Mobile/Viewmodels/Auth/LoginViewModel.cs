namespace BusTracking.Mobile.Viewmodels.Auth
{
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly IAppConfigService _appConfig;
        private bool _initialized;   // guard against InitializeAsync running twice

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private string _email = "";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private string _password = "";

        [ObservableProperty] private bool _isPasswordVisible;
        [ObservableProperty] private string _maintenanceMessage = "";
        [ObservableProperty] private bool _isMaintenanceMode;

        public LoginViewModel(IAuthService auth, INavigationService nav, IAppConfigService appConfig)
            : base(auth, nav)
        {
            Title = "Sign In";
            _appConfig = appConfig;
        }

        public override async Task InitializeAsync()
        {
            // Prevent running twice when Shell navigates back to Login
            if (_initialized) return;
            _initialized = true;

            await RunAsync(async () =>
            {
                // ── 1. Check maintenance mode ──────────────────────────────
                try
                {
                    var cfg = await _appConfig.GetMobileConfigAsync();

                    if (await _appConfig.IsMaintenanceModeAsync())
                    {
                        IsMaintenanceMode = true;
                        MaintenanceMessage = cfg.GetValueOrDefault("MaintenanceMessage",
                            "We are under maintenance. Please check back soon.");
                        return;
                    }
                }
                catch
                {
                    // API unreachable — skip maintenance check, show login form
                }

                // ── 2. Restore session if valid ────────────────────────────
                try
                {
                    if (await Auth.IsAuthenticatedAsync())
                    {
                        var user = await Auth.GetCurrentUserAsync();
                        if (user is not null)
                        {
                            await Nav.GoToDashboardAsync(user.Role);
                            return;
                        }
                    }
                }
                catch
                {
                    // Corrupt session — IsAuthenticatedAsync already cleared it
                    // Fall through to show login form normally
                }
            });
        }

        private bool CanLogin() =>
            !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);

        [RelayCommand(CanExecute = nameof(CanLogin))]
        private async Task LoginAsync()
        {
            await RunAsync(async () =>
            {
                var r = await Auth.LoginAsync(Email.Trim(), Password);
                if (!r.Success || r.Data is null)
                {
                    SetError(r.Message);
                    return;
                }

                // Clear fields after successful login
                Email = "";
                Password = "";

                await Nav.GoToDashboardAsync(r.Data.Role);
            });
        }

        [RelayCommand]
        private void TogglePasswordVisibility() => IsPasswordVisible = !IsPasswordVisible;

        [RelayCommand]
        private async Task ForgotPasswordAsync()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                await ShowAlertAsync("Email Required", "Please enter your email address first.");
                return;
            }
            await RunAsync(async () =>
            {
                var r = await Auth.ForgotPasswordAsync(Email.Trim());
                await ShowAlertAsync(r.Success ? "Email Sent" : "Error",
                    r.Success ? "Password reset link sent to your email." : r.Message);
            });
        }
    }
}