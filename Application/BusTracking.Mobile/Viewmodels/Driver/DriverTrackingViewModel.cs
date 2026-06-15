namespace BusTracking.Mobile.Viewmodels.Driver
{
    [QueryProperty(nameof(TripId), "TripId")]
    public partial class DriverTrackingViewModel : BaseViewModel
    {
        private readonly IDriverTripService _driverTrip;
        private readonly IBackgroundLocationService _bgLocation;
        private readonly ITrackingHubService _hub;

        [ObservableProperty] private int _tripId;
        [ObservableProperty] private string _tripStatus = "Loading…";
        [ObservableProperty] private string _tripTypeLabel = "";
        [ObservableProperty] private bool _isInProgress;
        [ObservableProperty] private string _gpsStatus = "Connecting…";
        [ObservableProperty] private ObservableCollection<DriverTripStop> _stops = [];
        [ObservableProperty] private DriverTripStop? _selectedStop;

        public DriverTrackingViewModel(
            IAuthService auth, INavigationService nav,
            IDriverTripService driverTrip,
            IBackgroundLocationService bgLocation,
            ITrackingHubService hub) : base(auth, nav)
        {
            Title = "Live Tracking";
            _driverTrip = driverTrip;
            _bgLocation = bgLocation;
            _hub = hub;
        }

        public override Task InitializeAsync() => LoadStopsCommand.ExecuteAsync(null);

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

        // ── Called by DriverTrackingPage.OnAppearing ──────────────────────
        // Starts background GPS + SignalR connection
        public async Task StartGpsTimer()
        {
            try
            {
                // Connect SignalR hub
                var user = await Auth.GetCurrentUserAsync();
                if (user is not null)
                {
                    await _hub.ConnectAsync(user.Token);
                    await _hub.JoinAsDriverAsync(TripId);
                }

                // Start background GPS — fires even when screen is locked
                await _bgLocation.StartAsync(TripId, OnLocationReceived);
                GpsStatus = "● Live";
            }
            catch (Exception ex)
            {
                GpsStatus = "GPS Error";
                SetError(ex.Message);
            }
        }

        // ── Called by DriverTrackingPage.OnDisappearing ───────────────────
        public async void StopGpsTimer()
        {
            await _bgLocation.StopAsync();
            GpsStatus = "Stopped";
        }

        // ── GPS callback — called every 5s even when screen is off ────────
        private async void OnLocationReceived(
            double lat, double lng,
            double? speed, double? heading)
        {
            GpsStatus = $"● Live  {(speed.HasValue ? $"{(int)speed} km/h" : "")}";

            // Send via SignalR (broadcasts to all watchers instantly)
            if (_hub.IsConnected)
            {
                await _hub.SendLocationAsync(
                    TripId, 0,
                    (decimal)lat, (decimal)lng,
                    speed.HasValue ? (decimal?)speed : null,
                    heading.HasValue ? (decimal?)heading : null);
            }
            else
            {
                // Fallback: REST ping if SignalR is not connected
                await _driverTrip.PingLocationAsync(TripId, new LocationPingRequest
                {
                    Latitude = lat,
                    Longitude = lng,
                    Speed = speed,
                    Heading = heading
                });
            }
        }

        // ── Mark stop Reached ─────────────────────────────────────────────
        [RelayCommand]
        private async Task MarkReachedAsync(DriverTripStop stop)
        {
            if (stop.Status != "Pending") return;
            await RunAsync(async () =>
            {
                stop.Status = "Reached";
                OnPropertyChanged(nameof(Stops));
                await LoadStopsAsync();
            });
        }

        // ── Mark stop Departed ────────────────────────────────────────────
        [RelayCommand]
        private async Task MarkDepartedAsync(DriverTripStop stop)
        {
            if (stop.Status != "Reached") return;
            await RunAsync(async () =>
            {
                stop.Status = "Departed";
                OnPropertyChanged(nameof(Stops));
                await LoadStopsAsync();
            });
        }

        // ── Update student boarding ───────────────────────────────────────
        [RelayCommand]
        private async Task UpdateBoardingAsync(
            (DriverStudentStatus student, string status) args)
        {
            var (student, status) = args;
            await RunAsync(async () =>
            {
                if (SelectedStop is null) return;
                var r = await _driverTrip.UpdateBoardingAsync(TripId,
                    new UpdateBoardingRequest
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

        // ── End trip ──────────────────────────────────────────────────────
        [RelayCommand]
        private async Task EndTripAsync()
        {
            if (!await ConfirmAsync("End Trip",
                    "End this trip? GPS tracking will stop for all parents."))
                return;

            await RunAsync(async () =>
            {
                await _bgLocation.StopAsync();
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
