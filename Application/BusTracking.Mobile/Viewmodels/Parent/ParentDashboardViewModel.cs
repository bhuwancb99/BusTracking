namespace BusTracking.Mobile.Viewmodels.Parent
{
    public partial class ParentDashboardViewModel : BaseViewModel
    {
        private readonly IParentService _parents;
        private readonly IAcademicYearService _academicYearService;

        [ObservableProperty] private ObservableCollection<LinkedStudent> _children = [];
        [ObservableProperty] private string _welcomeText = "";
        [ObservableProperty] private string _todayLabel = "";
        [ObservableProperty] private string _childrenCountLabel = "0 children";
        [ObservableProperty] private string _selectedSessionName = "Session: Loading...";
        [ObservableProperty] private List<AcademicYearItem> _academicYears = new();

        public ParentDashboardViewModel(
            IAuthService auth,
            INavigationService nav,
            IParentService parents,
            IAcademicYearService academicYearService)
            : base(auth, nav)
        {
            _parents = parents;
            _academicYearService = academicYearService;
            Title = "Parent Portal";
        }

        public override async Task InitializeAsync()
        {
            var user = await Auth.GetCurrentUserAsync();
            WelcomeText = $"Welcome back, {user?.FullName?.Split(' ')?.FirstOrDefault() ?? ""}";
            TodayLabel = DateTime.Today.ToString("dddd, dd MMMM yyyy");
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
                    var raw = await _parents.GetDashboardAsync();

                    if (raw is System.Text.Json.JsonElement je && je.ValueKind == System.Text.Json.JsonValueKind.Object)
                    {
                        if (je.TryGetProperty("children", out var childrenEl) ||
                            je.TryGetProperty("Students", out childrenEl) ||
                            je.TryGetProperty("students", out childrenEl))
                        {
                            var list = System.Text.Json.JsonSerializer.Deserialize<List<LinkedStudent>>(
                                childrenEl.GetRawText(),
                                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            Children = new ObservableCollection<LinkedStudent>(list ?? []);
                        }
                    }

                    ChildrenCountLabel = Children.Count == 1 ? "1 child" : $"{Children.Count} children";
                });
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand] private Task TrackChildAsync(LinkedStudent child) => Nav.GoToAsync("//ParentTracking", new Dictionary<string, object> { ["StudentId"] = child.StudentId });
        [RelayCommand] private Task ViewChildAvailabilityAsync(LinkedStudent child) => Nav.GoToAsync("//ParentAvailability", new Dictionary<string, object> { ["StudentId"] = child.StudentId });
        [RelayCommand] private Task GoToNotificationAsync() => Nav.GoToAsync("//ParentNotification");

        [RelayCommand]
        private async Task LogoutAsync()
        {
            if (!await ConfirmAsync("Logout", "Are you sure?")) return;
            await Auth.LogoutAsync();
            await Nav.GoToLoginAsync();
        }
    }
}
