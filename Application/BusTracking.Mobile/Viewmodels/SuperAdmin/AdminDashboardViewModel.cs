namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminDashboardViewModel : BaseViewModel
    {
        private readonly IDashboardService _dash;

        [ObservableProperty] private DashboardSummary? _summary;
        [ObservableProperty] private string _welcomeText = "";
        [ObservableProperty] private string _todayDate = "";

        public AdminDashboardViewModel(IAuthService auth, INavigationService nav, IDashboardService dash)
            : base(auth, nav) { _dash = dash; Title = "Dashboard"; }

        public override async Task InitializeAsync()
        {
            var user = await Auth.GetCurrentUserAsync();
            WelcomeText = $"Welcome back, {user?.FullName ?? "Admin"}";
            TodayDate = DateTime.Now.ToString("dddd, dd MMMM yyyy");
            await RefreshCommand.ExecuteAsync(null);
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
        // List pages are ShellContent — must use // absolute prefix
        [RelayCommand] private Task GoToBusesAsync() => Nav.GoToAsync("//AdminBusList");
        [RelayCommand] private Task GoToDriversAsync() => Nav.GoToAsync("//AdminDriverList");
        [RelayCommand] private Task GoToStudentsAsync() => Nav.GoToAsync("//AdminStudentList");
        [RelayCommand] private Task GoToParentsAsync() => Nav.GoToAsync("//AdminParentList");
        [RelayCommand] private Task GoToRoutesAsync() => Nav.GoToAsync("//AdminRouteList");
        [RelayCommand] private Task GoToTripsAsync() => Nav.GoToAsync("//AdminTripList");
        [RelayCommand] private Task GoToCoordinatorsAsync() => Nav.GoToAsync("//AdminCoordinatorList");
        [RelayCommand] private Task GoToConfigAsync() => Nav.GoToAsync("//AdminConfigList");

        // ── Quick Actions (jump directly to Add form) ─────────────────────
        [RelayCommand] private Task QuickAddCoordinatorAsync() => Nav.GoToAsync("AdminCoordinatorForm");
        [RelayCommand] private Task QuickAddDriverAsync() => Nav.GoToAsync("AdminDriverForm");
        [RelayCommand] private Task QuickAddStudentAsync() => Nav.GoToAsync("AdminStudentForm");
        [RelayCommand] private Task QuickAddParentAsync() => Nav.GoToAsync("AdminParentForm");
        [RelayCommand] private Task QuickAddBusAsync() => Nav.GoToAsync("AdminBusForm");
    }
}