namespace BusTracking.Mobile.Viewmodels.Shared
{
    /// <summary>
    /// Single shared live-tracking ViewModel used by ALL roles:
    ///
    ///   Role          Navigate with          How it loads
    ///   ──────────    ─────────────────      ──────────────────────────────────
    ///   Student       StudentId              ITripService / IStudentService.GetTrackingAsync
    ///   Parent        StudentId              IParentService.TrackChildBusAsync
    ///   Driver        TripId                 ITripService.GetLocationAsync & GetByIdAsync
    ///   Coordinator   TripId                 ITripService.GetLocationAsync & GetByIdAsync
    ///   SuperAdmin    TripId                 ITripService.GetLocationAsync & GetByIdAsync
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

        private CancellationTokenSource? _pollingCts;

        // ── Query params ──────────────────────────────────────────────────
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
                    IsLive = status is null || status.Contains("Live", StringComparison.OrdinalIgnoreCase);
                });
        }

        partial void OnTripIdChanged(int value)
        {
            if (value > 0)
            {
                _ = LoadInitialDataAsync();
            }
        }

        partial void OnStudentIdChanged(int value)
        {
            if (value > 0)
            {
                _ = LoadInitialDataAsync();
            }
        }

        public override async Task InitializeAsync()
        {
            if (TripId > 0 || StudentId > 0)
            {
                await LoadInitialDataAsync();
            }
        }

        // ── Load initial bus info + stops + position for ALL roles ─────────
        private async Task LoadInitialDataAsync()
        {
            await RunAsync(async () =>
            {
                TrackingData? data = null;

                if (StudentId > 0)
                {
                    // Parent or Student role
                    data = Auth.CurrentRole == "Parent"
                        ? await _parents.TrackChildBusAsync(StudentId)
                        : await _students.GetTrackingAsync();

                    if (data != null)
                    {
                        BusName = data.Bus?.BusName ?? "School Bus";
                        BusNumber = data.Bus?.BusNumber ?? "";
                        DriverName = data.Trip?.DriverName ?? "";
                        if (data.Trip != null && data.Trip.TripId > 0)
                        {
                            TripId = data.Trip.TripId;
                        }
                        if (data.Stops != null)
                        {
                            Stops = new ObservableCollection<StopStatus>(data.Stops);
                        }

                        // Draw stops on map
                        if (data.Stops != null && data.Stops.Any())
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
                            LastUpdateLabel = $"Updated {data.Location.RecordedAt:HH:mm:ss}";
                            IsLive = true;
                            ConnectionStatus = "● Live";
                        }
                    }
                }
                else if (TripId > 0)
                {
                    // Driver, Coordinator, SuperAdmin
                    var trip = await _trips.GetByIdAsync(TripId);
                    if (trip != null)
                    {
                        BusName = trip.RouteName ?? "School Bus";
                        BusNumber = trip.BusNumber ?? "";
                        DriverName = trip.DriverName ?? "";
                    }

                    var loc = await _trips.GetLocationAsync(TripId);
                    if (loc is not null)
                    {
                        SendToMap?.Invoke(
                            $"window.moveBus({loc.Latitude:F6}, {loc.Longitude:F6}, 0)");
                        SpeedLabel = loc.SpeedDisplay;
                        LastUpdateLabel = $"Updated {loc.RecordedAt:HH:mm:ss}";
                        IsLive = true;
                        ConnectionStatus = "● Live";
                    }
                }

                // Connect SignalR & Start Polling Timer for active trip
                if (TripId > 0)
                {
                    await ConnectAndWatchAsync(TripId);
                    StartPolling(TripId);
                }
            });
        }

        // ── SignalR: connect and join trip group ──────────────────────────
        private async Task ConnectAndWatchAsync(int tripId)
        {
            try
            {
                var user = await Auth.GetCurrentUserAsync();
                if (user is null) return;
                await _hub.ConnectAsync(user.Token);
                await _hub.WatchTripAsync(tripId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LiveTrackingViewModel] ConnectAndWatchAsync error: {ex.Message}");
            }
        }

        // ── Periodic Polling Fallback (3s interval) ──────────────────────
        private void StartPolling(int tripId)
        {
            StopPolling();
            _pollingCts = new CancellationTokenSource();
            var token = _pollingCts.Token;

            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(3000, token);
                        if (token.IsCancellationRequested) break;

                        var loc = await _trips.GetLocationAsync(tripId);
                        if (loc is not null && !token.IsCancellationRequested)
                        {
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                SpeedLabel = loc.SpeedDisplay;
                                LastUpdateLabel = $"Updated {loc.RecordedAt:HH:mm:ss}";
                                IsLive = true;
                                ConnectionStatus = "● Live";
                                SendToMap?.Invoke($"window.moveBus({loc.Latitude:F6}, {loc.Longitude:F6}, 0)");
                            });
                        }
                    }
                    catch (OperationCanceledException) { break; }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[LiveTrackingViewModel] Polling exception: {ex.Message}");
                    }
                }
            }, token);
        }

        private void StopPolling()
        {
            try
            {
                _pollingCts?.Cancel();
                _pollingCts?.Dispose();
                _pollingCts = null;
            }
            catch { }
        }

        // ── Receive real-time location push via SignalR ───────────────────
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
                StopPolling();
                SendToMap?.Invoke("window.onTripEnded()");
            });
        }

        // ── Called from code-behind OnDisappearing ────────────────────────
        public void Cleanup()
        {
            StopPolling();
            _hub.OnLocationReceived -= OnBusLocationReceived;
            _hub.OnTripEnded -= OnTripEnded;
            if (TripId > 0)
                _ = _hub.StopWatchingAsync(TripId);
        }

        [RelayCommand]
        private Task GoBackAsync() => Nav.GoBackAsync();
    }
}
