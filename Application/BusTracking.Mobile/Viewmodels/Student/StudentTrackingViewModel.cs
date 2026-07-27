namespace BusTracking.Mobile.Viewmodels.Student
{
    public partial class StudentTrackingViewModel : BaseViewModel
    {
        private readonly IStudentService _students;
        private System.Timers.Timer? _pollTimer;

        [ObservableProperty] private TrackingData? _tracking;
        [ObservableProperty] private LinkedStudent? _student;
        [ObservableProperty] private string _statusLabel = "Loading…";
        [ObservableProperty] private bool _isLive;
        [ObservableProperty] private double _busLatitude;
        [ObservableProperty] private double _busLongitude;
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

                if (data?.Student != null)
                {
                    Student = data.Student;
                }

                if (IsLive && data?.Location is not null)
                {
                    BusLatitude = (double)data.Location.Latitude;
                    BusLongitude = (double)data.Location.Longitude;
                    StatusLabel = $"{data.Bus?.BusNumber ?? data.Bus?.BusName ?? "Bus"} — Moving";
                    SendToMap?.Invoke($"window.moveBus({data.Location.Latitude:F6}, {data.Location.Longitude:F6}, 0)");
                }
                else
                {
                    StatusLabel = data?.Message ?? "No active trip";
                }

                if (data?.Stops != null && data.Stops.Any())
                {
                    var childStopName = Student?.StopName ?? data.StudentStop?.StopName;
                    var childStopId = Student?.StopId ?? data.StudentStop?.StopId;

                    foreach (var s in data.Stops)
                    {
                        if ((childStopId.HasValue && childStopId.Value > 0 && s.StopId == childStopId.Value) ||
                            (!string.IsNullOrEmpty(childStopName) && s.StopName.Equals(childStopName, StringComparison.OrdinalIgnoreCase)))
                        {
                            s.IsChildStop = true;
                            s.ChildStudentName = Student?.FullName ?? "Your";
                        }
                    }

                    Stops = new ObservableCollection<StopStatus>(data.Stops);

                    var json = JsonSerializer.Serialize(
                        data.Stops.Select(s => new
                        {
                            lat = s.Latitude,
                            lng = s.Longitude,
                            label = s.StopName,
                            order = s.StopOrder,
                            status = s.Status,
                            isChildStop = s.IsChildStop,
                            childName = s.ChildStudentName ?? "Your"
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
