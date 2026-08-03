namespace BusTracking.Mobile.Viewmodels.Teacher
{
    public partial class TeacherDashboardViewModel : BaseViewModel
    {
        private readonly ITeacherService _teacherService;

        [ObservableProperty] private TeacherItem? _profile;
        [ObservableProperty] private string _greetingMessage = "Welcome!";

        public TeacherDashboardViewModel(IAuthService auth, INavigationService nav, ITeacherService teacherService)
            : base(auth, nav)
        {
            Title = "Teacher Dashboard";
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
            });
        }

        [RelayCommand]
        private async Task GoToProfileAsync() => await Nav.GoToAsync("Profile");

        [RelayCommand]
        private async Task RefreshAsync() => await InitializeAsync();
    }
}
