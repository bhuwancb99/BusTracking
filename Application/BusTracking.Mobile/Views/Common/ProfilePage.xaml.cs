namespace BusTracking.Mobile.Views.Common;

/// <summary>
/// Code-behind for ProfilePage.
/// Follows the same thin pattern used across this project (ViewBase&lt;TViewModel&gt;).
/// All business logic, API calls, and state live in ProfileViewModel.
/// </summary>
public partial class ProfilePage : ViewBase<ProfileViewModel>
{
    public ProfilePage(ProfileViewModel vm) : base(vm) => InitializeComponent();
}
