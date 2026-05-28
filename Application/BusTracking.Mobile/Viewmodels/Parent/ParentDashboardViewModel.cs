namespace BusTracking.Mobile.Viewmodels.Parent
{
    public partial class ParentDashboardViewModel : BaseViewModel
    {
        private readonly IParentService _parents;
        private readonly IStudentService _students;

        [ObservableProperty] private ObservableCollection<LinkedStudent> _children = [];
        [ObservableProperty] private string _welcomeText = "";

        public ParentDashboardViewModel(IAuthService auth, INavigationService nav,
            IParentService parents, IStudentService students)
            : base(auth, nav) { _parents = parents; _students = students; Title = "My Dashboard"; }

        public override async Task InitializeAsync()
        {
            var user = await Auth.GetCurrentUserAsync();
            WelcomeText = $"Welcome, {user?.FullName?.Split(' ')[0] ?? ""}";
            await RefreshCommand.ExecuteAsync(null);
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await RunAsync(async () =>
            {
                var dashboard = await _parents.GetDashboardAsync();
                // Parse children from dashboard response
                // For now, we get from the linked student model
            });
        }

        [RelayCommand]
        private Task TrackChildAsync(LinkedStudent child) =>
            Nav.GoToAsync("ParentTracking", new Dictionary<string, object> { ["StudentId"] = child.StudentId });

        [RelayCommand]
        private async Task LogoutAsync()
        {
            if (!await ConfirmAsync("Logout", "Are you sure?")) return;
            await Auth.LogoutAsync();
            await Nav.GoToLoginAsync();
        }
    }
}
