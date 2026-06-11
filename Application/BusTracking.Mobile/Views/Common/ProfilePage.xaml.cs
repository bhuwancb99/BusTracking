namespace BusTracking.Mobile.Views.Common;

/// <summary>
/// Code-behind for ProfilePage — intentionally thin.
/// All logic lives in ProfileViewModel (CommunityToolkit MVVM).
/// ViewBase sets BindingContext = vm and calls InitializeAsync / RefreshOnReturnAsync.
/// </summary>
public partial class ProfilePage : ViewBase<ProfileViewModel>
{
    public ProfilePage(ProfileViewModel vm) : base(vm) => InitializeComponent();
}
