namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordinatorDashboardViewModel : BaseViewModel
    {
        private readonly IDashboardService _dash;
        private readonly IAcademicYearService _academicYearService;

        [ObservableProperty] private DashboardSummary? _summary;
        [ObservableProperty] private string _welcomeText = "";
        [ObservableProperty] private string _todayDate = "";
        [ObservableProperty] private string _selectedSessionName = "Session: Loading...";
        [ObservableProperty] private List<AcademicYearItem> _academicYears = new();

        public bool ShowRoutes => Can("route.view");
        public bool ShowBuses => Can("bus.view");
        public bool ShowDrivers => Can("driver.view");
        public bool ShowParents => Can("parent.view");
        public bool ShowStudents => Can("student.view");
        public bool ShowTrips => Can("trip.view") || Can("trip.manage");
        public bool ShowNotifs => Can("notification.manage");
        public bool ShowSupport => Can("helpsupport.view") || Can("helpsupport.manage");

        public bool CanAddStudent => Can("student.add");
        public bool CanAddBus => Can("bus.add");
        public bool CanCreateTrip => Can("trip.manage");

        public CoordinatorDashboardViewModel(IAuthService auth, INavigationService nav, IDashboardService dash, IAcademicYearService academicYearService)
            : base(auth, nav)
        {
            _dash = dash;
            _academicYearService = academicYearService;
            Title = "Coordinator Dashboard";
        }

        public override async Task InitializeAsync()
        {
            var user = await Auth.GetCurrentUserAsync();
            WelcomeText = $"Hi, {user?.FullName?.Split(' ')?.FirstOrDefault() ?? "Coordinator"}";
            TodayDate = DateTime.Now.ToString("dddd, dd MMMM yyyy");

            NotifyPermissionsChanged();
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
                AcademicYears = await _academicYearService.GetAcademicYearsAsync(isCoordinator: true);
                var active = AcademicYears.FirstOrDefault(a => a.IsCurrent) 
                             ?? await _academicYearService.GetActiveAcademicYearAsync(isCoordinator: true);
                
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
                var years = await _academicYearService.GetAcademicYearsAsync(isCoordinator: true);
                if (years == null || years.Count == 0)
                {
                    await ShowAlertAsync("Session Selection", "No academic years found.");
                    return;
                }

                AcademicYears = years;
                var options = years.Select(y => y.IsCurrent ? $"{y.YearName} (Active)" : y.YearName).ToArray();

                if (Application.Current?.Windows[0].Page is Page page)
                {
                    string selected = await page.DisplayActionSheet("Select Academic Session", "Cancel", null, options);
                    if (string.IsNullOrWhiteSpace(selected) || selected == "Cancel") return;

                    string cleanName = selected.Replace(" (Active)", "").Trim();
                    var item = years.FirstOrDefault(y => y.YearName.Equals(cleanName, StringComparison.OrdinalIgnoreCase));
                    
                    if (item != null && !item.IsCurrent)
                    {
                        var res = await _academicYearService.SetActiveAcademicYearAsync(item.AcademicYearId, isCoordinator: true);
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
                    Summary = await _dash.GetAdminSummaryAsync(forceRefresh: true);
                });
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private void NotifyPermissionsChanged()
        {
            OnPropertyChanged(nameof(ShowRoutes));
            OnPropertyChanged(nameof(ShowBuses));
            OnPropertyChanged(nameof(ShowDrivers));
            OnPropertyChanged(nameof(ShowParents));
            OnPropertyChanged(nameof(ShowStudents));
            OnPropertyChanged(nameof(ShowTrips));
            OnPropertyChanged(nameof(ShowNotifs));
            OnPropertyChanged(nameof(ShowSupport));
            OnPropertyChanged(nameof(CanAddStudent));
            OnPropertyChanged(nameof(CanAddBus));
            OnPropertyChanged(nameof(CanCreateTrip));
        }

        [RelayCommand] private Task GoToRoutesAsync() => Nav.GoToAsync("//CoordRouteList");
        [RelayCommand] private Task GoToBusesAsync() => Nav.GoToAsync("//CoordBusList");
        [RelayCommand] private Task GoToDriversAsync() => Nav.GoToAsync("//CoordDriverList");
        [RelayCommand] private Task GoToParentsAsync() => Nav.GoToAsync("//CoordParentList");
        [RelayCommand] private Task GoToStudentsAsync() => Nav.GoToAsync("//CoordStudentList");
        [RelayCommand] private Task GoToTripsAsync() => Nav.GoToAsync("//CoordTripList");
        [RelayCommand] private Task GoToNotificationAsync() => Nav.GoToAsync("//CoordNotificationList");

        [RelayCommand] private Task QuickAddDriverAsync() => Nav.GoToAsync("CoordDriverForm");
        [RelayCommand] private Task QuickAddStudentAsync() => Nav.GoToAsync("CoordStudentForm");
        [RelayCommand] private Task QuickAddParentAsync() => Nav.GoToAsync("CoordParentForm");
        [RelayCommand] private Task QuickAddBusAsync() => Nav.GoToAsync("CoordBusForm");

        [RelayCommand]
        private async Task LogoutAsync()
        {
            if (!await ConfirmAsync("Logout", "Are you sure you want to logout?")) return;
            await Auth.LogoutAsync();
            await Nav.GoToLoginAsync();
        }
    }
}