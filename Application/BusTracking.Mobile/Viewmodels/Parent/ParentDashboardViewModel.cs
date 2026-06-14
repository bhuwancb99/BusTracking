namespace BusTracking.Mobile.Viewmodels.Parent
{
    public partial class ParentDashboardViewModel : BaseViewModel
    {
        private readonly IParentService _parents;

        [ObservableProperty] private ObservableCollection<LinkedStudent> _children = [];
        [ObservableProperty] private string _welcomeText = "";
        [ObservableProperty] private string _todayLabel = "";
        [ObservableProperty] private string _childrenCountLabel = "0 children";

        public ParentDashboardViewModel(IAuthService auth, INavigationService nav, IParentService parents)
            : base(auth, nav) { _parents = parents; Title = "Parent Portal"; }

        public override async Task InitializeAsync()
        {
            var user = await Auth.GetCurrentUserAsync();
            WelcomeText = $"Welcome back, {user?.FullName?.Split(' ')[0] ?? ""} 👋";
            TodayLabel = DateTime.Today.ToString("dddd, dd MMMM yyyy");
            await RefreshCommand.ExecuteAsync(null);
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await RunAsync(async () =>
            {
                // Load children linked to this parent via dashboard API
                var raw = await _parents.GetDashboardAsync();

                // Deserialize from JSON if the API returns children inside dashboard
                // Fallback: try to extract LinkedStudent list from the raw object
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

        // Navigate to tracking — if multiple children, go to dashboard's tracking tab;
        // if called from child card, pass that specific student
        [RelayCommand]
        private Task TrackChildAsync(LinkedStudent child) =>
            Shell.Current.GoToAsync("///ParentTracking");

        [RelayCommand]
        private Task LeaveChildAsync(LinkedStudent child) =>
            Shell.Current.GoToAsync("///ParentAvailability");

        [RelayCommand]
        private Task GoToTrackingAsync() => Shell.Current.GoToAsync("//ParentTracking");

        [RelayCommand]
        private Task GoToAvailabilityAsync() => Shell.Current.GoToAsync("//ParentAvailability");

        [RelayCommand]
        private Task GoToFeedbackAsync() => Shell.Current.GoToAsync("//ParentFeedback");

        [RelayCommand]
        private async Task LogoutAsync()
        {
            if (!await ConfirmAsync("Logout", "Are you sure?")) return;
            await Auth.LogoutAsync();
            await Nav.GoToLoginAsync();
        }
    }
}
