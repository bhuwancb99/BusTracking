using BusTracking.Mobile.Interfaces;
using BusTracking.Mobile.Models.Dashboard;
using BusTracking.Mobile.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminDashboardViewModel : BaseViewModel
    {
        private readonly IDashboardService _dash;

        [ObservableProperty] private DashboardSummary? _summary;
        [ObservableProperty] private string _welcomeText = "";

        public AdminDashboardViewModel(IAuthService auth, INavigationService nav, IDashboardService dash)
            : base(auth, nav) { _dash = dash; Title = "Dashboard"; }

        public override async Task InitializeAsync()
        {
            var user = await Auth.GetCurrentUserAsync();
            WelcomeText = $"Welcome, {user?.FullName?.Split(' ')[0] ?? "Admin"}";
            await RefreshCommand.ExecuteAsync(null);
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await RunAsync(async () =>
            {
                Summary = await _dash.GetAdminSummaryAsync(forceRefresh: true);
            });
        }

        [RelayCommand] private Task GoToBusesAsync() => Nav.GoToAsync("AdminBusList");
        [RelayCommand] private Task GoToDriversAsync() => Nav.GoToAsync("AdminDriverList");
        [RelayCommand] private Task GoToStudentsAsync() => Nav.GoToAsync("AdminStudentList");
        [RelayCommand] private Task GoToParentsAsync() => Nav.GoToAsync("AdminParentList");
        [RelayCommand] private Task GoToRoutesAsync() => Nav.GoToAsync("AdminRouteList");
        [RelayCommand] private Task GoToTripsAsync() => Nav.GoToAsync("AdminTripList");
        [RelayCommand] private Task GoToCoordinatorsAsync() => Nav.GoToAsync("AdminCoordinatorList");
        [RelayCommand] private Task GoToConfigAsync() => Nav.GoToAsync("AdminConfigList");

        [RelayCommand]
        private async Task LogoutAsync()
        {
            if (!await ConfirmAsync("Logout", "Are you sure you want to logout?")) return;
            await Auth.LogoutAsync();
            await Nav.GoToLoginAsync();
        }
    }
}
