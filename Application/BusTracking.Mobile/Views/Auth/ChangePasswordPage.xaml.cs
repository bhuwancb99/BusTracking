using BusTracking.Mobile.Viewmodels.Auth;

namespace BusTracking.Mobile.Views.Auth;

public partial class ChangePasswordPage : ViewBase<ChangePasswordViewModel>
{
    public ChangePasswordPage(ChangePasswordViewModel vm) : base(vm) => InitializeComponent();
}