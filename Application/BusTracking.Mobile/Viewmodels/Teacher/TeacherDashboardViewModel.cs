namespace BusTracking.Mobile.Viewmodels.Teacher
{
    public partial class TeacherDashboardViewModel : BaseViewModel
    {
        private readonly ITeacherService _teacherService;

        [ObservableProperty] private TeacherItem? _profile;
        [ObservableProperty] private string _greetingMessage = "Welcome!";
        [ObservableProperty] private string _welcomeText = "Welcome Back!";
        [ObservableProperty] private string _todayDate = DateTime.Now.ToString("dddd, dd MMMM yyyy");

        public TeacherDashboardViewModel(IAuthService auth, INavigationService nav, ITeacherService teacherService)
            : base(auth, nav)
        {
            Title = "Teacher Portal";
            _teacherService = teacherService;
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
                await CheckNotificationPermissionAsync();
            });
        }

        [RelayCommand]
        private async Task GoToProfileAsync() => await Nav.GoToAsync("//Profile");

        [RelayCommand]
        private async Task GoToNotificationAsync() => await Nav.GoToAsync("//TeacherNotification");

        [RelayCommand]
        private async Task RefreshAsync() => await InitializeAsync();
    }
}
