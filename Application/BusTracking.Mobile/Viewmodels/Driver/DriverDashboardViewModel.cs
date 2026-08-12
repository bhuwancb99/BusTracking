namespace BusTracking.Mobile.Viewmodels.Driver
{
    public partial class DriverDashboardViewModel : BaseViewModel
    {
        private readonly IDriverTripService _driverTrip;
        private readonly IAcademicYearService _academicYearService;

        [ObservableProperty] private string _welcomeText = "";
        [ObservableProperty] private string _busDisplay = "No bus assigned";
        [ObservableProperty] private string _routeDisplay = "No route";
        [ObservableProperty] private int _totalStudents;
        [ObservableProperty] private bool _hasTrip;
        [ObservableProperty] private bool _hasActiveTrip;
        [ObservableProperty] private DriverTripItem? _activeTrip;
        [ObservableProperty] private string _todayDate = "";
        [ObservableProperty] private string _selectedSessionName = "Session: Loading...";
        [ObservableProperty] private List<AcademicYearItem> _academicYears = new();

        public DriverDashboardViewModel(
            IAuthService auth,
            INavigationService nav,
            IDriverTripService driverTrip,
            IAcademicYearService academicYearService)
            : base(auth, nav)
        {
            Title = "Driver Dashboard";
            _driverTrip = driverTrip;
            _academicYearService = academicYearService;
        }

        public override async Task InitializeAsync()
        {
            var user = await Auth.GetCurrentUserAsync();
            WelcomeText = $"Hi, {user?.FullName?.Split(' ')?.FirstOrDefault() ?? "Driver"}";
            TodayDate = DateTime.Now.ToString("dddd, dd MMMM yyyy");
            await LoadActiveSessionAsync();
            await CheckNotificationPermissionAsync(requestIfFirstTime: true);
            await RefreshCommand.ExecuteAsync(null);
        }

        public override async Task RefreshOnReturnAsync()
        {
            await LoadActiveSessionAsync();
            await CheckNotificationPermissionAsync(requestIfFirstTime: false);
            await RefreshCommand.ExecuteAsync(null);
        }

        private async Task LoadActiveSessionAsync()
        {
            try
            {
                AcademicYears = await _academicYearService.GetAcademicYearsAsync();
                var active = AcademicYears.FirstOrDefault(a => a.IsCurrent)
                             ?? await _academicYearService.GetActiveAcademicYearAsync();

                SelectedSessionName = active != null ? $"Session: {active.YearName}" : "Select Session";
            }
            catch
            {
                SelectedSessionName = "Session: 2026-2027";
            }
        }

        [RelayCommand]
        private async Task SelectSessionAsync()
        {
            try
            {
                var years = await _academicYearService.GetAcademicYearsAsync();
                if (years == null || years.Count == 0)
                {
                    await ShowAlertAsync("Session Selection", "No academic years found.");
                    return;
                }

                AcademicYears = years;
                var options = years.Select(y => y.IsCurrent ? $"{y.YearName} (Active)" : y.YearName).ToArray();

                if (Application.Current?.Windows[0].Page is Page page)
                {
                    string selected = await page.DisplayActionSheetAsync("Select Academic Session", "Cancel", null, options);
                    if (string.IsNullOrWhiteSpace(selected) || selected == "Cancel") return;

                    string cleanName = selected.Replace(" (Active)", "").Trim();
                    var item = years.FirstOrDefault(y => y.YearName.Equals(cleanName, StringComparison.OrdinalIgnoreCase));

                    if (item != null && !item.IsCurrent)
                    {
                        var res = await _academicYearService.SetActiveAcademicYearAsync(item.AcademicYearId);
                        if (res.Success)
                        {
                            SelectedSessionName = $"Session: {item.YearName}";
                            await ShowToastAsync($"Active session changed to {item.YearName}");
                            await RefreshCommand.ExecuteAsync(null);
                        }
                        else
                        {
                            SetError(res.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
            }
        }

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
        private Task GoToNotificationAsync() => Nav.GoToAsync("//DriverNotification");

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