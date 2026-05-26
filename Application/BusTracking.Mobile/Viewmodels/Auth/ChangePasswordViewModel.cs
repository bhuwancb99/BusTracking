using BusTracking.Mobile.Interfaces;
using BusTracking.Mobile.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BusTracking.Mobile.Viewmodels.Auth
{
    public partial class ChangePasswordViewModel : BaseViewModel
    {
        [ObservableProperty] private string _currentPassword = "";
        [ObservableProperty] private string _newPassword = "";
        [ObservableProperty] private string _confirmPassword = "";

        public ChangePasswordViewModel(IAuthService auth, INavigationService nav)
            : base(auth, nav) => Title = "Change Password";

        [RelayCommand]
        private async Task SubmitAsync()
        {
            if (NewPassword != ConfirmPassword)
            {
                SetError("New password and confirm password do not match.");
                return;
            }
            if (NewPassword.Length < 6)
            {
                SetError("Password must be at least 6 characters.");
                return;
            }
            await RunAsync(async () =>
            {
                var r = await Auth.ChangePasswordAsync(CurrentPassword, NewPassword);
                if (r.Success)
                {
                    await ShowToastAsync("Password changed successfully.");
                    await Nav.GoBackAsync();
                }
                else
                    SetError(r.Message);
            });
        }
    }
}
