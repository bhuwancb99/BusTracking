namespace BusTracking.Mobile.Viewmodels.Driver
{
    [QueryProperty(nameof(TripId), "TripId")]
    public partial class DriverTrackingViewModel : BaseViewModel
    {
        private readonly IDriverTripService _driverTrip;
        private readonly IBackgroundLocationService _bgLocation;
        private readonly ITrackingHubService _hub;
        private IDispatcherTimer? _gpsTimer;

        [ObservableProperty] private int _tripId;
        [ObservableProperty] private string _tripStatus = "Loading…";
        [ObservableProperty] private string _tripTypeLabel = "";
        [ObservableProperty] private bool _isInProgress;
        [ObservableProperty] private ObservableCollection<DriverTripStop> _stops = [];
        [ObservableProperty] private DriverTripStop? _selectedStop;

        // ── Permission state ──────────────────────────────────────────────
        [ObservableProperty] private bool _locationGranted;
        [ObservableProperty] private bool _locationDeniedPermanently;
        [ObservableProperty] private string _gpsStatus = "";

        /// <summary>True when location is denied — bottom banner is visible.</summary>
        public bool ShowLocationBanner => !LocationGranted;

        /// <summary>
        /// "Enable Location" when permission can still be asked.
        /// "Open Settings" when permanently denied.
        /// </summary>
        public string LocationBannerButtonText =>
            LocationDeniedPermanently ? "Open Settings" : "Enable Location";

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
        public async Task StartGpsTimer()
        {
            var granted = await RequestLocationPermissionAsync();
            if (!granted) return;

            // Connect SignalR
            var user = await Auth.GetCurrentUserAsync();
            if (user is not null)
            {
                await _hub.ConnectAsync(user.Token);
                await _hub.JoinAsDriverAsync(TripId);
            }

            // Start background GPS
            await _bgLocation.StartAsync(TripId, OnLocationReceived);
            GpsStatus = "● Live";
        }

        // ── Called by DriverTrackingPage.OnDisappearing ───────────────────
        public void StopGpsTimer()
        {
            _gpsTimer?.Stop();
            _ = _bgLocation.StopAsync();
            GpsStatus = "";
        }

        // ── Permission request — called on page appear and Enable button ──
        [RelayCommand]
        public async Task RequestLocationAsync()
        {
            var granted = await RequestLocationPermissionAsync();

            if (granted)
            {
                // Permission just granted — start GPS and refresh the page
                await StartGpsTimer();
                await LoadStopsCommand.ExecuteAsync(null);
            }
            else if (LocationDeniedPermanently)
            {
                // Permanently denied — send driver to Settings
                if (AppInfo.Current.ShowSettingsUI != null)
                    AppInfo.Current.ShowSettingsUI();
            }
        }

        // ── Core permission logic ─────────────────────────────────────────
        private async Task<bool> RequestLocationPermissionAsync()
        {
            // Check current status first (no prompt if already granted)
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status == PermissionStatus.Granted)
            {
                // Android 10+ also needs background permission
                status = await EnsureBackgroundLocationAsync();
            }

            if (status != PermissionStatus.Granted)
            {
                // Request the permission — system dialog appears
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            if (status == PermissionStatus.Granted)
                status = await EnsureBackgroundLocationAsync();

            LocationGranted = status == PermissionStatus.Granted;
            LocationDeniedPermanently = status == PermissionStatus.Denied;
            OnPropertyChanged(nameof(ShowLocationBanner));
            OnPropertyChanged(nameof(LocationBannerButtonText));

            if (!LocationGranted)
                SetError("Location permission is required for live tracking.");

            return LocationGranted;
        }

        private static async Task<PermissionStatus> EnsureBackgroundLocationAsync()
        {
#if ANDROID
            if (OperatingSystem.IsAndroidVersionAtLeast(29))
            {
                var bg = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
                if (bg != PermissionStatus.Granted)
                    bg = await Permissions.RequestAsync<Permissions.LocationAlways>();
                return bg;
            }
#endif
            return PermissionStatus.Granted;
        }

        // ── GPS callback — every 5s even when screen off ──────────────────
        private async void OnLocationReceived(
            double lat, double lng,
            double? speed, double? heading)
        {
            GpsStatus = $"● Live  {(speed.HasValue ? $"{(int)speed} km/h" : "")}";

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
                    "End this trip? All tracking will stop for parents."))
                return;

            await RunAsync(async () =>
            {
                StopGpsTimer();
                await _hub.NotifyTripEndedAsync(TripId);
                await _hub.DisconnectAsync();

                var r = await _driverTrip.EndTripAsync(TripId);
                if (r.Success)
                {
                    await ShowToastAsync("Trip ended successfully.");
                    await Nav.GoToAsync("//DriverDashboard");
                }
                else SetError(r.Message);
            });
        }

        [RelayCommand]
        private Task GoBackAsync() => Nav.GoBackAsync();
    }
}
