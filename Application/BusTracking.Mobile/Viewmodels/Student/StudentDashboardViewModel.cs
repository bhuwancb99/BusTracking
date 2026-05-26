using BusTracking.Mobile.Interfaces;
using BusTracking.Mobile.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BusTracking.Mobile.Viewmodels.Student
{
    public partial class StudentDashboardViewModel : BaseViewModel
    {
        private readonly IStudentService _students;

        [ObservableProperty] private string _welcomeText = "";
        [ObservableProperty] private string _busDisplay = "No bus assigned";
        [ObservableProperty] private string _stopDisplay = "No stop";
        [ObservableProperty] private string _tripStatus = "No active trip";
        [ObservableProperty] private bool _hasActiveTrip;
        [ObservableProperty] private int? _activeTripId;

        public StudentDashboardViewModel(IAuthService auth, INavigationService nav, IStudentService students)
            : base(auth, nav) { _students = students; Title = "My Dashboard"; }

        public override async Task InitializeAsync()
        {
            var user = await Auth.GetCurrentUserAsync();
            WelcomeText = $"Hi, {user?.FullName?.Split(' ')[0] ?? ""}";
            await RefreshCommand.ExecuteAsync(null);
        }

        [RelayCommand]
        private async Task RefreshAsync()
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

        [ObservableProperty] private bool _isLive;

        [RelayCommand] private Task TrackBusAsync() => Nav.GoToAsync("StudentTracking");
        [RelayCommand] private Task ViewAvailabilityAsync() => Nav.GoToAsync("StudentAvailability");

        [RelayCommand]
        private async Task LogoutAsync()
        {
            if (!await ConfirmAsync("Logout", "Are you sure?")) return;
            await Auth.LogoutAsync();
            await Nav.GoToLoginAsync();
        }
    }
}
