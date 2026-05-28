namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordinatorDashboardViewModel : BaseViewModel
    {
        private readonly IDashboardService _dash;

        [ObservableProperty] private DashboardSummary? _summary;
        [ObservableProperty] private string _welcomeText = "";

        // Menu visibility based on permissions
        public bool ShowRoutes => Can("route.view");
        public bool ShowBuses => Can("bus.view");
        public bool ShowDrivers => Can("driver.view");
        public bool ShowParents => Can("parent.view");
        public bool ShowStudents => Can("student.view");
        public bool ShowTrips => Can("trip.view") || Can("trip.manage");
        public bool ShowNotifs => Can("notification.manage");
        public bool ShowSupport => Can("helpsupport.view") || Can("helpsupport.manage");

        // Quick action visibility
        public bool CanAddStudent => Can("student.add");
        public bool CanAddBus => Can("bus.add");
        public bool CanCreateTrip => Can("trip.manage");

        public CoordinatorDashboardViewModel(IAuthService auth, INavigationService nav, IDashboardService dash)
            : base(auth, nav) { _dash = dash; Title = "Coordinator Dashboard"; }

        public override async Task InitializeAsync()
        {
            var user = await Auth.GetCurrentUserAsync();
            WelcomeText = $"Hi, {user?.FullName?.Split(' ')[0] ?? "Coordinator"}";
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

        [RelayCommand] private Task GoToRoutesAsync() => Nav.GoToAsync("CoordRouteList");
        [RelayCommand] private Task GoToBusesAsync() => Nav.GoToAsync("CoordBusList");
        [RelayCommand] private Task GoToDriversAsync() => Nav.GoToAsync("CoordDriverList");
        [RelayCommand] private Task GoToParentsAsync() => Nav.GoToAsync("CoordParentList");
        [RelayCommand] private Task GoToStudentsAsync() => Nav.GoToAsync("CoordStudentList");
        [RelayCommand] private Task GoToTripsAsync() => Nav.GoToAsync("CoordTripList");

        [RelayCommand]
        private async Task LogoutAsync()
        {
            if (!await ConfirmAsync("Logout", "Are you sure you want to logout?")) return;
            await Auth.LogoutAsync();
            await Nav.GoToLoginAsync();
        }
    }
}
