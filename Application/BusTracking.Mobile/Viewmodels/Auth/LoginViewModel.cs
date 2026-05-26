using BusTracking.Mobile.Interfaces;
using BusTracking.Mobile.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BusTracking.Mobile.Viewmodels.Auth
{
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly IAppConfigService _appConfig;

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
            // On app start: fetch mobile config, check maintenance
            await RunAsync(async () =>
            {
                var cfg = await _appConfig.GetMobileConfigAsync();

                if (await _appConfig.IsMaintenanceModeAsync())
                {
                    IsMaintenanceMode = true;
                    MaintenanceMessage = cfg.GetValueOrDefault("MaintenanceMessage",
                        "We are under maintenance. Please check back soon.");
                    return;
                }

                // If valid session exists, skip login
                if (await Auth.IsAuthenticatedAsync())
                {
                    var user = await Auth.GetCurrentUserAsync();
                    if (user is not null)
                        await Nav.GoToDashboardAsync(user.Role);
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
