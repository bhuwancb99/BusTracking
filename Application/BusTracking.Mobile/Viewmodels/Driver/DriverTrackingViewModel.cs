namespace BusTracking.Mobile.Viewmodels.Driver
{
    /// <summary>
    ///   - Replaced HTTP-polling GPS timer with SignalR SendLocation call.
    ///   - Added ConnectionStatus label so driver can see "Connected / Reconnecting".
    ///   - On EndTrip → calls TripEnded on hub → all parent apps show "Trip Ended" banner.
    ///   - GPS still polled locally every 5s using Geolocation, but result is sent
    ///     via SignalR hub (which also saves to DB), so REST LocationController.Ping
    ///     is no longer needed from the app (kept for backward compat on server).
    /// </summary>
    [QueryProperty(nameof(TripId), "TripId")]
    [QueryProperty(nameof(BusId), "BusId")]
    public partial class DriverTrackingViewModel : BaseViewModel
    {
        private readonly IDriverTripService _driverTrip;
        private readonly ITrackingHubService _hub;
        private readonly IAuthService _auth;
        private CancellationTokenSource? _gpsCts;

        [ObservableProperty] private int _tripId;
        [ObservableProperty] private int _busId;
        [ObservableProperty] private string _tripStatus = "Loading…";
        [ObservableProperty] private string _tripTypeLabel = "";
        [ObservableProperty] private bool _isInProgress;
        [ObservableProperty] private string _connectionStatus = "";
        [ObservableProperty] private bool _isConnected;
        [ObservableProperty] private ObservableCollection<DriverTripStop> _stops = [];
        [ObservableProperty] private DriverTripStop? _selectedStop;

        public DriverTrackingViewModel(
            IAuthService auth, INavigationService nav,
            IDriverTripService driverTrip,
            ITrackingHubService hub) : base(auth, nav)
        {
            Title = "Live Tracking";
            _auth = auth;
            _driverTrip = driverTrip;
            _hub = hub;

            _hub.OnConnectionStateChanged += status =>
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ConnectionStatus = status ?? "Live";
                    IsConnected = status is null;
                });
        }

        public override async Task InitializeAsync()
        {
            await ConnectHubAsync();
            await LoadStopsCommand.ExecuteAsync(null);
            StartGpsTimer();
        }

        // ── SignalR connect + join driver group ───────────────────────────
        private async Task ConnectHubAsync()
        {
            ConnectionStatus = "Connecting…";
            var user = await _auth.GetCurrentUserAsync();
            if (user is null) return;
            await _hub.ConnectAsync(user.Token);
            await _hub.JoinAsDriverAsync(TripId);
            IsConnected = true;
            ConnectionStatus = "Live";
        }

        // ── Load stops & students ─────────────────────────────────────────
        [RelayCommand]
        private async Task LoadStopsAsync()
        {
            await RunAsync(async () =>
            {
                var list = await _driverTrip.GetTripStopsAsync(TripId);
                Stops = new ObservableCollection<DriverTripStop>(list);
                IsEmpty = Stops.Count == 0;
                SelectedStop = Stops.FirstOrDefault(s => s.Status != "Departed")
                            ?? Stops.FirstOrDefault();
            });
        }

        // ── Mark stop Reached / Departed ─────────────────────────────────
        [RelayCommand]
        private async Task MarkReachedAsync(DriverTripStop stop)
        {
            if (stop.Status != "Pending") return;
            await RunAsync(async () => { stop.Status = "Reached"; await LoadStopsAsync(); });
        }

        [RelayCommand]
        private async Task MarkDepartedAsync(DriverTripStop stop)
        {
            if (stop.Status != "Reached") return;
            await RunAsync(async () => { stop.Status = "Departed"; await LoadStopsAsync(); });
        }

        // ── Student boarding ──────────────────────────────────────────────
        [RelayCommand]
        private async Task UpdateBoardingAsync((DriverStudentStatus student, string status) args)
        {
            var (student, status) = args;
            await RunAsync(async () =>
            {
                if (SelectedStop is null) return;
                var r = await _driverTrip.UpdateBoardingAsync(TripId, new UpdateBoardingRequest
                {
                    StudentId = student.StudentId,
                    StopId = SelectedStop.StopId,
                    BoardingStatus = status
                });
                if (r.Success)
                {
                    student.BoardingStatus = status;
                    await ShowToastAsync($"{student.StudentName}: {status}");
                    await LoadStopsAsync();
                }
                else SetError(r.Message);
            });
        }

        // ── GPS → SignalR broadcast every 5 seconds ───────────────────────
        public void StartGpsTimer()
        {
            _gpsCts = new CancellationTokenSource();
            var token = _gpsCts.Token;

            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    await BroadcastCurrentLocationAsync();
                    try { await Task.Delay(5_000, token); }
                    catch (TaskCanceledException) { break; }
                }
            }, token);
        }

        public void StopGpsTimer() => _gpsCts?.Cancel();

        private async Task BroadcastCurrentLocationAsync()
        {
            try
            {
                var loc = await Geolocation.GetLocationAsync(
                    new GeolocationRequest(GeolocationAccuracy.Best,
                                          TimeSpan.FromSeconds(4)));
                if (loc is null || !_hub.IsConnected) return;

                await _hub.SendLocationAsync(
                    TripId, BusId,
                    (decimal)loc.Latitude,
                    (decimal)loc.Longitude,
                    loc.Speed.HasValue ? (decimal?)loc.Speed : null,
                    loc.Course.HasValue ? (decimal?)loc.Course : null);
            }
            catch { /* GPS errors are non-fatal */ }
        }

        // ── End trip ──────────────────────────────────────────────────────
        [RelayCommand]
        private async Task EndTripAsync()
        {
            if (!await ConfirmAsync("End Trip", "This will stop live tracking for all parents."))
                return;

            await RunAsync(async () =>
            {
                _gpsCts?.Cancel();
                await _hub.NotifyTripEndedAsync(TripId);
                await _hub.DisconnectAsync();

                var r = await _driverTrip.EndTripAsync(TripId);
                if (r.Success)
                {
                    await ShowToastAsync("Trip ended. Parents have been notified.");
                    await Nav.GoToAsync("//DriverDashboard");
                }
                else SetError(r.Message);
            });
        }

        [RelayCommand]
        private Task GoBackAsync() => Nav.GoBackAsync();
    }
}
