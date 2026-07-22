namespace BusTracking.Mobile.Viewmodels.Shared
{
    /// <summary>
    /// Single shared live-tracking ViewModel used by ALL roles:
    ///
    ///   Role          Navigate with          How it loads
    ///   ──────────    ─────────────────      ──────────────────────────────────
    ///   Student       StudentId              ITripService.GetLocationAsync via
    ///                                        IStudentService.GetTrackingAsync
    ///   Parent        StudentId              IParentService.TrackChildBusAsync
    ///   Driver        TripId                 ITripService.GetLocationAsync
    ///   Coordinator   TripId                 ITripService.GetLocationAsync
    ///   SuperAdmin    TripId                 ITripService.GetLocationAsync
    ///
    /// The page is reached by:
    ///   await Shell.Current.GoToAsync($"LiveTracking?TripId={id}");
    ///   await Shell.Current.GoToAsync($"LiveTracking?StudentId={id}");
    /// </summary>
    [QueryProperty(nameof(TripId), "TripId")]
    [QueryProperty(nameof(StudentId), "StudentId")]
    public partial class LiveTrackingViewModel : BaseViewModel
    {
        private readonly ITrackingHubService _hub;
        private readonly ITripService _trips;
        private readonly IStudentService _students;
        private readonly IParentService _parents;

        // ── Query params (only one will be set per role) ──────────────────
        [ObservableProperty] private int _tripId;
        [ObservableProperty] private int _studentId;

        // ── Display properties ────────────────────────────────────────────
        [ObservableProperty] private string _busName = "";
        [ObservableProperty] private string _busNumber = "";
        [ObservableProperty] private string _driverName = "";
        [ObservableProperty] private string _speedLabel = "";
        [ObservableProperty] private string _lastUpdateLabel = "";
        [ObservableProperty] private string _connectionStatus = "Connecting…";
        [ObservableProperty] private bool _isLive;
        [ObservableProperty] private bool _tripEnded;
        [ObservableProperty] private bool _isSheetExpanded;
        [ObservableProperty] private ObservableCollection<StopStatus> _stops = [];

        [RelayCommand]
        private void ToggleSheet()
        {
            IsSheetExpanded = !IsSheetExpanded;
        }

        // JS bridge wired by code-behind
        public Action<string>? SendToMap { get; set; }

        public LiveTrackingViewModel(
            IAuthService auth, INavigationService nav,
            ITrackingHubService hub,
            ITripService trips,
            IStudentService students,
            IParentService parents)
            : base(auth, nav)
        {
            Title = "Live Tracking";
            _hub = hub;
            _trips = trips;
            _students = students;
            _parents = parents;

            _hub.OnLocationReceived += OnBusLocationReceived;
            _hub.OnTripEnded += OnTripEnded;
            _hub.OnConnectionStateChanged += status =>
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ConnectionStatus = status ?? "● Live";
                    IsLive = status is null;
                });
        }

        public override async Task InitializeAsync()
        {
            await LoadInitialDataAsync();
            if (TripId > 0)
                await ConnectAndWatchAsync(TripId);
        }

        // ── Load initial bus info + stops by role ─────────────────────────
        private async Task LoadInitialDataAsync()
        {
            await RunAsync(async () =>
            {
                TrackingData? data = null;

                if (StudentId > 0)
                {
                    // Parent or Student — use TrackChildBusAsync / GetTrackingAsync
                    data = StudentId > 0 && Auth.CurrentRole == "Parent"
                        ? await _parents.TrackChildBusAsync(StudentId)
                        : await _students.GetTrackingAsync();
                }
                else if (TripId > 0)
                {
                    // Driver, Coordinator, SuperAdmin — load from ITripService
                    var loc = await _trips.GetLocationAsync(TripId);
                    if (loc is not null)
                    {
                        SendToMap?.Invoke(
                            $"window.moveBus({loc.Latitude:F6}, {loc.Longitude:F6}, 0)");
                        SpeedLabel = loc.SpeedDisplay;
                        LastUpdateLabel = $"Updated {loc.RecordedAt:HH:mm:ss}";
                    }
                    return; // stops not loaded here — Driver has DriverTrackingPage for that
                }

                if (data is null) return;

                BusName = data.Bus?.BusName ?? "";
                BusNumber = data.Bus?.BusNumber ?? "";
                TripId = data.Trip?.TripId ?? 0;
                Stops = new ObservableCollection<StopStatus>(data.Stops);

                // Draw stops on map
                if (data.Stops.Any())
                {
                    var json = System.Text.Json.JsonSerializer.Serialize(
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

                // Show last known bus position immediately
                if (data.Location is not null)
                {
                    SendToMap?.Invoke(
                        $"window.moveBus({data.Location.Latitude:F6}, {data.Location.Longitude:F6}, 0)");
                    SpeedLabel = data.Location.SpeedDisplay;
                }

                // Connect SignalR now that we have the TripId
                if (TripId > 0)
                    await ConnectAndWatchAsync(TripId);
            });
        }

        // ── SignalR: connect and join trip group ──────────────────────────
        private async Task ConnectAndWatchAsync(int tripId)
        {
            var user = await Auth.GetCurrentUserAsync();
            if (user is null) return;
            await _hub.ConnectAsync(user.Token);
            await _hub.WatchTripAsync(tripId);
        }

        // ── Receive real-time location push ───────────────────────────────
        private void OnBusLocationReceived(
            decimal lat, decimal lng,
            decimal? speed, decimal? heading, string time)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                SpeedLabel = speed.HasValue ? $"{(int)speed} km/h" : "";
                LastUpdateLabel = $"Updated {DateTime.Now:HH:mm:ss}";
                IsLive = true;
                ConnectionStatus = "● Live";
                SendToMap?.Invoke(
                    $"window.moveBus({lat:F6}, {lng:F6}, {(int)(heading ?? 0)})");
            });
        }

        // ── Driver ended the trip ─────────────────────────────────────────
        private void OnTripEnded(int tripId)
        {
            if (tripId != TripId) return;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                TripEnded = true;
                ConnectionStatus = "Trip Ended";
                IsLive = false;
                SendToMap?.Invoke("window.onTripEnded()");
            });
        }

        // ── Called from code-behind OnDisappearing ────────────────────────
        public void Cleanup()
        {
            _hub.OnLocationReceived -= OnBusLocationReceived;
            _hub.OnTripEnded -= OnTripEnded;
            if (TripId > 0)
                _ = _hub.StopWatchingAsync(TripId);
        }

        [RelayCommand]
        private Task GoBackAsync() => Nav.GoBackAsync();
    }
}
