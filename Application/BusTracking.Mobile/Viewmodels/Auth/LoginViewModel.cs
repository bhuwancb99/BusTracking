namespace BusTracking.Mobile.Viewmodels.Auth
{
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly IGlobalConfigService _globalConfig;
        private bool _initialized;   // guard against InitializeAsync running twice

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private string _userName = "";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private string _password = "";

        [ObservableProperty] private bool _isPasswordVisible;

        /// <summary>Inverse of IsPasswordVisible — bound to Entry.IsPassword.</summary>
        public bool IsPasswordHidden => !IsPasswordVisible;

        partial void OnIsPasswordVisibleChanged(bool value) =>
            OnPropertyChanged(nameof(IsPasswordHidden));

        public LoginViewModel(IAuthService auth, INavigationService nav, IGlobalConfigService globalConfig)
            : base(auth, nav)
        {
            Title = "Sign In";
            _globalConfig = globalConfig;
        }

        public override async Task InitializeAsync()
        {
            // Prevent running twice when Shell navigates back to Login
            if (_initialized) return;
            _initialized = true;

            await RunAsync(async () =>
            {
                // ── 1. Check maintenance mode & version update from GlobalConfigurations ───
                try
                {
                    var cfg = await _globalConfig.GetGlobalConfigAsync(forceRefresh: true);

                    if (await _globalConfig.IsMaintenanceModeAsync())
                    {
                        // Navigate to full-screen maintenance page
                        await NavigateToMaintenanceAsync(cfg);
                        return;
                    }

                    await CheckAppVersionAndUpdateAsync(cfg);
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

        /// <summary>
        /// Navigates to the full-screen MaintenancePage passing the custom message.
        /// </summary>
        private async Task NavigateToMaintenanceAsync(Dictionary<string, string> cfg)
        {
            string message = cfg.GetValueOrDefault("MaintenanceMessage",
                "We are under maintenance. Please check back soon.");

            await Shell.Current.GoToAsync("//Maintenance", true);

            // Set the message on the MaintenancePage after navigation
            if (Shell.Current.CurrentPage is Views.Common.MaintenancePage mp)
            {
                mp.SetMessage(message);
            }
        }

        private bool CanLogin() =>
            !string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrWhiteSpace(Password);

        [RelayCommand(CanExecute = nameof(CanLogin))]
        private async Task LoginAsync()
        {
            await RunAsync(async () =>
            {
                var r = await Auth.LoginAsync(UserName.Trim(), Password);
                if (!r.Success || r.Data is null)
                {
                    SetError(r.Message);
                    return;
                }

                // Clear fields after successful login
                UserName = "";
                Password = "";

                await Nav.GoToDashboardAsync(r.Data.Role);
            });
        }

        [RelayCommand]
        private void TogglePasswordVisibility() => IsPasswordVisible = !IsPasswordVisible;

        [RelayCommand]
        private async Task ForgotPasswordAsync()
        {
            await ShowAlertAsync("Forgot Password", "Please contact your administrator to reset your password.");
        }

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

                        // Show custom UpdatePopup
                        if (Application.Current?.Windows[0].Page is Page page)
                        {
                            while (true)
                            {
                                var popup = new Views.Common.UpdatePopup(
                                    currentVersionStr, targetVersionStr, updateUrl, isMandatory);

                                var result = await page.ShowPopupAsync<string>(popup);

                                if (isMandatory)
                                {
                                    // Mandatory: popup re-shows after user returns from store
                                    // The while loop keeps showing it until user updates
                                    continue;
                                }
                                else
                                {
                                    // Optional: user tapped "Later" or "Update Now" — break either way
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
    }
}