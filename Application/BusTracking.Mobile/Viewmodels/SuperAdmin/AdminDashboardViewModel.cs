namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminDashboardViewModel : BaseViewModel
    {
        private readonly IDashboardService _dash;
        private readonly IAcademicYearService _academicYearService;

        [ObservableProperty] private DashboardSummary? _summary;
        [ObservableProperty] private string _welcomeText = "";
        [ObservableProperty] private string _todayDate = "";
        [ObservableProperty] private string _selectedSessionName = "Session: Loading...";
        [ObservableProperty] private List<AcademicYearItem> _academicYears = new();

        public AdminDashboardViewModel(IAuthService auth, INavigationService nav, IDashboardService dash, IAcademicYearService academicYearService)
            : base(auth, nav)
        {
            _dash = dash;
            _academicYearService = academicYearService;
            Title = "Dashboard";
        }

        public override async Task InitializeAsync()
        {
            var user = await Auth.GetCurrentUserAsync();
            WelcomeText = $"Welcome back, {user?.FullName ?? "Admin"}";
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
                AcademicYears = await _academicYearService.GetAcademicYearsAsync(isCoordinator: false);
                var active = AcademicYears.FirstOrDefault(a => a.IsCurrent)
                             ?? await _academicYearService.GetActiveAcademicYearAsync(isCoordinator: false);

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
                var years = await _academicYearService.GetAcademicYearsAsync(isCoordinator: false);
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
                        var res = await _academicYearService.SetActiveAcademicYearAsync(item.AcademicYearId, isCoordinator: false);
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

        // ── Navigation ────────────────────────────────────────────────────
        [RelayCommand] private Task GoToBusesAsync() => Nav.GoToAsync("//AdminBusList");
        [RelayCommand] private Task GoToDriversAsync() => Nav.GoToAsync("//AdminDriverList");
        [RelayCommand] private Task GoToStudentsAsync() => Nav.GoToAsync("//AdminStudentList");
        [RelayCommand] private Task GoToParentsAsync() => Nav.GoToAsync("//AdminParentList");
        [RelayCommand] private Task GoToRoutesAsync() => Nav.GoToAsync("//AdminRouteList");
        [RelayCommand] private Task GoToTripsAsync() => Nav.GoToAsync("//AdminTripList");
        [RelayCommand] private Task GoToCoordinatorsAsync() => Nav.GoToAsync("//AdminCoordinatorList");
        [RelayCommand] private Task GoToConfigAsync() => Nav.GoToAsync("//AdminConfigList");
        [RelayCommand] private Task GoToNotificationAsync() => Nav.GoToAsync("//AdminNotificationList");

        // ── Quick Actions ──────────────────────────────────────────────────
        [RelayCommand] private Task QuickAddCoordinatorAsync() => Nav.GoToAsync("AdminCoordinatorForm");
        [RelayCommand] private Task QuickAddDriverAsync() => Nav.GoToAsync("AdminDriverForm");
        [RelayCommand] private Task QuickAddStudentAsync() => Nav.GoToAsync("AdminStudentForm");
        [RelayCommand] private Task QuickAddParentAsync() => Nav.GoToAsync("AdminParentForm");
        [RelayCommand] private Task QuickAddBusAsync() => Nav.GoToAsync("AdminBusForm");
    }
}