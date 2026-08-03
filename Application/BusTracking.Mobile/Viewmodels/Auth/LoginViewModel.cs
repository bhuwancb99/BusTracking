namespace BusTracking.Mobile.Viewmodels.Auth
{
    public partial class LoginViewModel : BaseViewModel
    {
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

        public LoginViewModel(IAuthService auth, INavigationService nav)
            : base(auth, nav)
        {
            Title = "Sign In";
        }

        public override async Task InitializeAsync()
        {
            // Prevent running twice when Shell navigates back to Login
            if (_initialized) return;
            _initialized = true;

            await RunAsync(async () =>
            {
                // Maintenance & version checks already done by MaintenancePage (startup gate).
                // Here we only restore session if the user is already logged in.
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
    }
}