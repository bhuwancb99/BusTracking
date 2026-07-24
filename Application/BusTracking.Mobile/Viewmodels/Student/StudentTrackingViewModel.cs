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
        [ObservableProperty] private bool _isSheetExpanded;

        public Action<string>? SendToMap { get; set; }

        [RelayCommand]
        private void ToggleSheet() => IsSheetExpanded = !IsSheetExpanded;

        public StudentTrackingViewModel(IAuthService auth, INavigationService nav, IStudentService students)
            : base(auth, nav) { _students = students; Title = "Track My Bus"; }

        public override async Task InitializeAsync()
        {
            await PollAsync();
            _pollTimer = new System.Timers.Timer(5_000);
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
                    SendToMap?.Invoke($"window.moveBus({data.Location.Latitude:F6}, {data.Location.Longitude:F6}, 0)");
                }

                if (data?.Stops?.Any() == true)
                {
                    Stops = new ObservableCollection<StopStatus>(data.Stops);
                    var json = JsonSerializer.Serialize(
                        data.Stops.Select(s => new
                        {
                            lat = s.Latitude,
                            lng = s.Longitude,
                            label = s.StopName,
                            order = s.StopOrder,
                            status = s.Status
                        }));
                    SendToMap?.Invoke($"window.setRouteStops({json})");
                }
            });
        }

        public void StopPolling() => _pollTimer?.Stop();

        [RelayCommand]
        private Task OpenLiveMapAsync()
        {
            var tripId = Tracking?.Trip?.TripId ?? 0;
            if (tripId > 0)
            {
                return Nav.GoToAsync("LiveTracking", new Dictionary<string, object> { ["TripId"] = tripId });
            }
            else
            {
                return Nav.GoToAsync("LiveTracking", new Dictionary<string, object> { ["StudentId"] = 1 });
            }
        }
    }
}
