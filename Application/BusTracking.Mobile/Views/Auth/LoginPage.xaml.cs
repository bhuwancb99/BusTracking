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

        SetSystemBarsColor(
            lightStatusKey: "LoginPageBgLight",  // blue  — top status bar
            darkStatusKey: "LoginPageBgDark",
            lightNavBarKey: "LoginCardBgLight",  // white — bottom nav bar
            darkNavBarKey: "LoginCardBgDark",
            useLightStatusIcons: true,               // white icons on blue
            useLightNavIcons: false               // black icons on white
        );
    }
}