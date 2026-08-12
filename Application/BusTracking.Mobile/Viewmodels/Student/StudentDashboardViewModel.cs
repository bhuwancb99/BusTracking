namespace BusTracking.Mobile.Viewmodels.Student
{
    public partial class StudentDashboardViewModel : BaseViewModel
    {
        private readonly IStudentService _students;
        private readonly IAcademicYearService _academicYearService;

        [ObservableProperty] private string _welcomeText = "";
        [ObservableProperty] private string _busDisplay = "No bus assigned";
        [ObservableProperty] private string _stopDisplay = "No stop";
        [ObservableProperty] private string _tripStatus = "No active trip";
        [ObservableProperty] private bool _hasActiveTrip;
        [ObservableProperty] private int? _activeTripId;
        [ObservableProperty] private bool _isLive;
        [ObservableProperty] private string _selectedSessionName = "Session: Loading...";
        [ObservableProperty] private List<AcademicYearItem> _academicYears = new();

        public StudentDashboardViewModel(
            IAuthService auth,
            INavigationService nav,
            IStudentService students,
            IAcademicYearService academicYearService)
            : base(auth, nav)
        {
            _students = students;
            _academicYearService = academicYearService;
            Title = "My Dashboard";
        }

        public override async Task InitializeAsync()
        {
            var user = await Auth.GetCurrentUserAsync();
            WelcomeText = $"Hi, {user?.FullName?.Split(' ')?.FirstOrDefault() ?? ""}";
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
                    var data = await _students.GetTrackingAsync();
                    if (data is null) return;
                    IsLive = data.IsLive;
                    BusDisplay = data.Bus is not null ? $"{data.Bus.BusName} ({data.Bus.BusNumber})" : "No bus assigned";
                    HasActiveTrip = data.IsLive;
                    ActiveTripId = data.Trip?.TripId;
                    TripStatus = data.IsLive
                        ? $"🚌 Bus is on the way — {data.BoardingStatus}"
                        : data.Message ?? "No active trip right now";
                });
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand] private Task TrackBusAsync() => Nav.GoToAsync("//StudentTracking");
        [RelayCommand] private Task ViewAvailabilityAsync() => Nav.GoToAsync("//StudentAvailability");
        [RelayCommand] private Task GoToNotificationAsync() => Nav.GoToAsync("//StudentNotification");

        [RelayCommand]
        private async Task LogoutAsync()
        {
            if (!await ConfirmAsync("Logout", "Are you sure?")) return;
            await Auth.LogoutAsync();
            await Nav.GoToLoginAsync();
        }
    }
}
