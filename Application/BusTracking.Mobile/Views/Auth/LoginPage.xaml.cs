namespace BusTracking.Mobile.Views.Auth;

public partial class LoginPage : ViewBase<LoginViewModel>
{
    public LoginPage(LoginViewModel vm) : base(vm) => InitializeComponent();

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        Shell.SetNavBarIsVisible(this, false);
        Shell.SetTabBarIsVisible(this, false);
        Shell.SetFlyoutBehavior(this, FlyoutBehavior.Disabled);
    }
}