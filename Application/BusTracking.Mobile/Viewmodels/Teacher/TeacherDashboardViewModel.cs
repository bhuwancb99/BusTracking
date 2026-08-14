namespace BusTracking.Mobile.Viewmodels.Teacher
{
    public partial class TeacherDashboardViewModel : BaseViewModel
    {
        private readonly ITeacherService _teacherService;
        private readonly IAcademicYearService _academicYearService;

        [ObservableProperty] private TeacherItem? _profile;
        [ObservableProperty] private string _greetingMessage = "Welcome!";
        [ObservableProperty] private string _welcomeText = "Welcome Back!";
        [ObservableProperty] private string _todayDate = DateTime.Now.ToString("dddd, dd MMMM yyyy");
        [ObservableProperty] private string _selectedSessionName = "Session: Loading...";
        [ObservableProperty] private List<AcademicYearItem> _academicYears = new();

        public TeacherDashboardViewModel(
            IAuthService auth,
            INavigationService nav,
            ITeacherService teacherService,
            IAcademicYearService academicYearService)
            : base(auth, nav)
        {
            Title = "Teacher Portal";
            _teacherService = teacherService;
            _academicYearService = academicYearService;
        }

        public override async Task InitializeAsync()
        {
            await RunAsync(async () =>
            {
                var hour = DateTime.Now.Hour;
                GreetingMessage = hour switch
                {
                    < 12 => "Good Morning,",
                    < 17 => "Good Afternoon,",
                    _ => "Good Evening,"
                };

                Profile = await _teacherService.GetMyProfileAsync();
                if (Profile != null && !string.IsNullOrWhiteSpace(Profile.FullName))
                {
                    WelcomeText = $"{GreetingMessage} {Profile.FullName.Split(' ')[0]}";
                }
                else
                {
                    WelcomeText = $"{GreetingMessage} Teacher";
                }

                TodayDate = DateTime.Now.ToString("dddd, dd MMMM yyyy");
                await LoadActiveSessionAsync();
                await CheckNotificationPermissionAsync();
            });
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
                            await RefreshAsync();
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
        private async Task GoToProfileAsync() => await Nav.GoToAsync("//Profile");

        [RelayCommand]
        private async Task GoToNotificationAsync() => await Nav.GoToAsync("//TeacherNotification");

        [RelayCommand]
        private async Task GoToMarksEntryAsync() => await Nav.GoToAsync("TeacherMarksEntry");

        [RelayCommand]
        private async Task RefreshAsync() => await InitializeAsync();

    }
}
