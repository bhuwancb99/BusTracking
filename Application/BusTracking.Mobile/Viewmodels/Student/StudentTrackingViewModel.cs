using BusTracking.Mobile.Interfaces;
using BusTracking.Mobile.Models.Tracking;
using BusTracking.Mobile.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace BusTracking.Mobile.Viewmodels.Student
{
    public partial class StudentTrackingViewModel : BaseViewModel
    {
        private readonly IStudentService _students;
        private System.Timers.Timer? _pollTimer;

        [ObservableProperty] private TrackingData? _tracking;
        [ObservableProperty] private string _statusText = "Loading…";
        [ObservableProperty] private string _speedText = "– km/h";
        [ObservableProperty] private bool _isLive;
        [ObservableProperty] private double _busLat;
        [ObservableProperty] private double _busLng;
        [ObservableProperty] private ObservableCollection<StopStatus> _stops = [];

        public StudentTrackingViewModel(IAuthService auth, INavigationService nav, IStudentService students)
            : base(auth, nav) { _students = students; Title = "Track My Bus"; }

        public override async Task InitializeAsync()
        {
            await PollAsync();
            _pollTimer = new System.Timers.Timer(10_000);
            _pollTimer.Elapsed += async (_, _) => await PollAsync();
            _pollTimer.Start();
        }

        private async Task PollAsync()
        {
            var data = await _students.GetTrackingAsync();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Tracking = data;
                IsLive = data?.IsLive ?? false;
                StatusText = IsLive
                    ? $"🚌 {data?.Bus?.BusNumber} is on the way"
                    : data?.Message ?? "No active trip";
                if (IsLive && data?.Location is not null)
                {
                    BusLat = (double)data.Location.Latitude;
                    BusLng = (double)data.Location.Longitude;
                    SpeedText = data.Location.SpeedDisplay;
                }
                if (data?.Stops?.Any() == true)
                    Stops = new ObservableCollection<StopStatus>(data.Stops);
            });
        }

        public void StopPolling() => _pollTimer?.Stop();
    }
}
