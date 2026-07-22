namespace BusTracking.Mobile.Viewmodels.Driver
{
    public partial class DriverDashboardViewModel : BaseViewModel
    {
        private readonly IDriverTripService _driverTrip;

        [ObservableProperty] private string _welcomeText = "";
        [ObservableProperty] private string _busDisplay = "No bus assigned";
        [ObservableProperty] private string _routeDisplay = "No route";
        [ObservableProperty] private int _totalStudents;
        [ObservableProperty] private bool _hasTrip;
        [ObservableProperty] private bool _hasActiveTrip;
        [ObservableProperty] private DriverTripItem? _activeTrip;
        [ObservableProperty] private string _todayDate = "";

        public DriverDashboardViewModel(IAuthService auth, INavigationService nav,
            IDriverTripService driverTrip) : base(auth, nav)
        {
            Title = "Driver Dashboard";
            _driverTrip = driverTrip;
        }

        public override async Task InitializeAsync()
        {
            var user = await Auth.GetCurrentUserAsync();
            WelcomeText = $"Hi, {user?.FullName?.Split(' ')?.FirstOrDefault() ?? "Driver"}";
            TodayDate = DateTime.Now.ToString("dddd, dd MMMM yyyy");
            await RefreshCommand.ExecuteAsync(null);
        }

        public override Task RefreshOnReturnAsync() => RefreshCommand.ExecuteAsync(null);

        [RelayCommand]
        private async Task RefreshAsync()
        {
            IsRefreshing = true;
            try
            {
                await RunAsync(async () =>
                {
                    var data = await _driverTrip.GetDashboardAsync();
                    if (data is null)
                    {
                        ActiveTrip = null;
                        HasTrip = false;
                        HasActiveTrip = false;
                        return;
                    }

                    BusDisplay = string.IsNullOrWhiteSpace(data.BusNumber)
                        ? "No bus assigned"
                        : $"{data.BusName} ({data.BusNumber})";
                    RouteDisplay = string.IsNullOrWhiteSpace(data.RouteName) ? "No route" : data.RouteName;
                    TotalStudents = data.TotalStudents;
                    ActiveTrip = data.ActiveTrip;
                    HasTrip = data.ActiveTrip != null && data.ActiveTrip.TripId > 0;
                    HasActiveTrip = data.ActiveTrip?.Status == "InProgress";
                });
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        private Task ViewTripsAsync() => Nav.GoToAsync("DriverTripList");

        [RelayCommand]
        private async Task StartActiveTripAsync()
        {
            if (ActiveTrip is null) return;
            await RunAsync(async () =>
            {
                var r = await _driverTrip.StartTripAsync(ActiveTrip.TripId);
                if (r.Success)
                {
                    HasActiveTrip = true;
                    await ShowToastAsync("Trip started! GPS tracking is now active.");
                    await Nav.GoToAsync("DriverTracking",
                        new Dictionary<string, object> { ["TripId"] = ActiveTrip.TripId });
                }
                else
                    SetError(r.Message);
            });
        }

        [RelayCommand]
        private async Task GoToActiveTrackingAsync()
        {
            if (ActiveTrip is null) return;
            await Nav.GoToAsync("DriverTracking",
                new Dictionary<string, object> { ["TripId"] = ActiveTrip.TripId });
        }

        [RelayCommand]
        private async Task LogoutAsync()
        {
            if (!await ConfirmAsync("Logout", "Are you sure you want to log out?")) return;
            await Auth.LogoutAsync();
            await Nav.GoToLoginAsync();
        }
    }
}