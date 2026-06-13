namespace BusTracking.Mobile.Viewmodels.Parent
{
    [QueryProperty(nameof(StudentId), "StudentId")]
    public partial class ParentLiveTrackingViewModel : BaseViewModel
    {
        private readonly ITrackingHubService _hub;
        private readonly IParentService _parent;

        [ObservableProperty] private int _studentId;
        [ObservableProperty] private int _tripId;
        [ObservableProperty] private string _busName = "";
        [ObservableProperty] private string _busNumber = "";
        [ObservableProperty] private string _speedLabel = "";
        [ObservableProperty] private string _lastUpdateLabel = "";
        [ObservableProperty] private string _connectionStatus = "Connecting…";
        [ObservableProperty] private bool _isLive;
        [ObservableProperty] private bool _tripEnded;
        [ObservableProperty] private ObservableCollection<StopStatus> _stops = [];

        // Wired by code-behind: ViewModel calls this → WebView executes JS
        public Action<string>? SendToMap { get; set; }

        public ParentLiveTrackingViewModel(
            IAuthService auth, INavigationService nav,
            ITrackingHubService hub, IParentService parent)
            : base(auth, nav)
        {
            Title = "Bus Tracking";
            _hub = hub;
            _parent = parent;

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
            await LoadTrackingDataAsync();
            await ConnectAndWatchAsync();
        }

        // ── Load static trip info using existing TrackChildBusAsync ──────
        private async Task LoadTrackingDataAsync()
        {
            await RunAsync(async () =>
            {
                var data = await _parent.TrackChildBusAsync(StudentId);
                if (data is null) return;

                BusName = data.Bus?.BusName ?? "";
                BusNumber = data.Bus?.BusNumber ?? "";

                TripId = data.Trip?.TripId ?? 0;
                Stops = new ObservableCollection<StopStatus>(data.Stops);

                // Draw route stops on map
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

                // If there is already a known location, place the bus immediately
                if (data.Location is not null)
                {
                    var lat = data.Location.Latitude;
                    var lng = data.Location.Longitude;
                    SendToMap?.Invoke($"window.moveBus({lat:F6}, {lng:F6}, 0)");
                }
            });
        }

        // ── Connect SignalR and join the trip group ───────────────────────
        private async Task ConnectAndWatchAsync()
        {
            if (TripId == 0) return;
            var user = await Auth.GetCurrentUserAsync();
            if (user is null) return;
            await _hub.ConnectAsync(user.Token);
            await _hub.WatchTripAsync(TripId);
        }

        // ── Receive real-time location from SignalR ───────────────────────
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

        // ── Called by code-behind OnDisappearing ─────────────────────────
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
