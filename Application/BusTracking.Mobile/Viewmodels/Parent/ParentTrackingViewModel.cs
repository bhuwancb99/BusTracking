namespace BusTracking.Mobile.Viewmodels.Parent
{
    public partial class ParentTrackingViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IParentService _parents;
        private System.Timers.Timer? _pollTimer;
        private int _preselectedStudentId;

        [ObservableProperty] private ObservableCollection<LinkedStudent> _students = [];
        [ObservableProperty] private LinkedStudent? _selectedStudent;
        [ObservableProperty] private TrackingData? _tracking;
        [ObservableProperty] private string _statusLabel = "Loading\u2026";
        [ObservableProperty] private double _busLatitude;
        [ObservableProperty] private double _busLongitude;
        [ObservableProperty] private bool _isLive;

        public ParentTrackingViewModel(IAuthService auth, INavigationService nav, IParentService parents)
            : base(auth, nav)
        {
            _parents = parents;
            Title = "Track Bus";
        }

        // Receives optional StudentId when tapped from dashboard child card
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("StudentId", out var id))
                _preselectedStudentId = (int)id;
        }

        public override async Task InitializeAsync()
        {
            await LoadStudentsAsync();
            StartPolling();
        }

        // ── Load students list ─────────────────────────────────────────
        private async Task LoadStudentsAsync()
        {
            var raw = await _parents.GetDashboardAsync();
            if (raw is null) return;

            var list = new List<LinkedStudent>();
            if (raw is JsonElement je && je.ValueKind == JsonValueKind.Object)
            {
                JsonElement el;
                bool found = je.TryGetProperty("children", out el)
                          || je.TryGetProperty("students", out el)
                          || je.TryGetProperty("Students", out el);

                if (found)
                {
                    list = JsonSerializer.Deserialize<List<LinkedStudent>>(
                        el.GetRawText(),
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
                }
            }

            Students = new ObservableCollection<LinkedStudent>(list);

            // Default: use pre-selected id or fall back to first student
            if (_preselectedStudentId > 0)
                SelectedStudent = Students.FirstOrDefault(s => s.StudentId == _preselectedStudentId)
                               ?? Students.FirstOrDefault();
            else
                SelectedStudent = Students.FirstOrDefault();
        }

        // ── React to picker change ─────────────────────────────────────
        partial void OnSelectedStudentChanged(LinkedStudent? value)
        {
            if (value is null) return;

            StopPolling();

            // Reset state immediately so old data is not shown
            Tracking = null;
            IsLive = false;
            BusLatitude = 0;
            BusLongitude = 0;
            StatusLabel = $"Loading {value.FullName}\u2026";

            StartPolling();
        }

        // ── Polling ────────────────────────────────────────────────────
        private void StartPolling()
        {
            _ = PollAsync();          // fire immediately
            _pollTimer = new System.Timers.Timer(10_000);
            _pollTimer.Elapsed += async (_, _) => await PollAsync();
            _pollTimer.Start();
        }

        private async Task PollAsync()
        {
            if (SelectedStudent is null) return;

            var data = await _parents.TrackChildBusAsync(SelectedStudent.StudentId);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Tracking = data;
                IsLive = data?.IsLive ?? false;

                if (IsLive && data?.Location is not null)
                {
                    BusLatitude = (double)data.Location.Latitude;
                    BusLongitude = (double)data.Location.Longitude;
                    StatusLabel = $"{data.Bus?.BusNumber} \u2014 Moving";
                }
                else
                {
                    StatusLabel = data?.Message ?? "No active trip";
                }
            });
        }

        public void StopPolling()
        {
            _pollTimer?.Stop();
            _pollTimer?.Dispose();
            _pollTimer = null;
        }

        [RelayCommand]
        private Task OpenLiveMapAsync()
        {
            if (SelectedStudent is null) return Task.CompletedTask;

            var tripId = Tracking?.Trip?.TripId ?? 0;
            if (tripId > 0)
            {
                return Nav.GoToAsync("LiveTracking", new Dictionary<string, object> { ["TripId"] = tripId });
            }
            else
            {
                return Nav.GoToAsync("LiveTracking", new Dictionary<string, object> { ["StudentId"] = SelectedStudent.StudentId });
            }
        }
    }
}
