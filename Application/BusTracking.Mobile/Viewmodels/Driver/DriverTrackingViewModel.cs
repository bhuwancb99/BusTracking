namespace BusTracking.Mobile.Viewmodels.Driver
{
    [QueryProperty(nameof(TripId), "TripId")]
    public partial class DriverTrackingViewModel : BaseViewModel
    {
        private readonly IDriverTripService _driverTrip;
        private readonly IBackgroundLocationService _bgLocation;
        private readonly ITrackingHubService _hub;
        private IDispatcherTimer? _gpsTimer = null;

        [ObservableProperty] private int _tripId;
        [ObservableProperty] private string _tripStatus = "Loading…";
        [ObservableProperty] private string _tripTypeLabel = "";
        [ObservableProperty] private bool _isInProgress;
        [ObservableProperty] private ObservableCollection<DriverTripStop> _stops = [];
        [ObservableProperty] private DriverTripStop? _selectedStop;
        [ObservableProperty] private bool _isSheetExpanded; // Collapsed by default

        [RelayCommand]
        private void ToggleSheet() => IsSheetExpanded = !IsSheetExpanded;

        // JS bridge for Google Maps WebView
        public Action<string>? SendToMap { get; set; }

        // ── Permission state ──────────────────────────────────────────────
        [ObservableProperty] private bool _locationGranted;
        [ObservableProperty] private bool _locationDeniedPermanently;
        [ObservableProperty] private string _gpsStatus = "";
        private bool _isLoadingStops;

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
            if (_isLoadingStops) return;
            _isLoadingStops = true;
            await RunAsync(async () =>
            {
                try
                {
                    DriverStudentStatus.StatusChangedCallback = null;
                    var stopsList = await _driverTrip.GetTripStopsAsync(TripId);
                    var studentsList = await _driverTrip.GetTripStudentsAsync(TripId);

                    // Group students under their respective stop and assign StopId
                    foreach (var stop in stopsList)
                    {
                        stop.Students = studentsList
                            .Where(st => st.StopOrder == stop.StopOrder ||
                                         (st.StopName != null && st.StopName.Equals(stop.StopName, StringComparison.OrdinalIgnoreCase)))
                            .ToList();

                        foreach (var st in stop.Students)
                        {
                            st.StopId = stop.StopId;
                        }
                    }

                    Stops = new ObservableCollection<DriverTripStop>(stopsList);
                    IsEmpty = Stops.Count == 0;
                    SelectedStop = Stops.FirstOrDefault(s => s.Status != "Departed")
                                ?? Stops.FirstOrDefault();

                    UpdateMapStops();

                    DriverStudentStatus.StatusChangedCallback = (student) =>
                    {
                        MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            await ChangeStudentStatusAsync(student);
                        });
                    };
                }
                finally
                {
                    _isLoadingStops = false;
                }
            });
        }

        private void UpdateMapStops()
        {
            if (Stops.Count == 0 || SendToMap is null) return;
            try
            {
                var mapStops = Stops.Select(s => new
                {
                    order = s.StopOrder,
                    label = s.StopName,
                    lat = (double)(s.Latitude ?? 0),
                    lng = (double)(s.Longitude ?? 0),
                    status = s.Status
                }).ToList();

                var stopsJson = JsonSerializer.Serialize(mapStops);
                SendToMap.Invoke($"setRouteStops({stopsJson})");
            }
            catch { }
        }

        // ── Called by DriverTrackingPage.OnAppearing ──────────────────────
        public async Task StartGpsTimer()
        {
            var granted = await RequestLocationPermissionAsync();
            if (!granted) return;

            // Immediately query location so vehicle icon shows on map instantly
            try
            {
                var loc = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(3)))
                       ?? await Geolocation.GetLastKnownLocationAsync();
                if (loc != null)
                {
                    double? speedKmh = loc.Speed.HasValue && loc.Speed.Value > 0 ? loc.Speed.Value * 3.6 : (double?)null;
                    OnLocationReceived(loc.Latitude, loc.Longitude, speedKmh, loc.Course);
                }
            }
            catch { }

            // Connect SignalR
            var user = await Auth.GetCurrentUserAsync();
            if (user is not null)
            {
                await _hub.ConnectAsync(user.Token);
                await _hub.JoinAsDriverAsync(TripId);
            }

            // Start background GPS
            await _bgLocation.StartAsync(TripId, OnLocationReceived);
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
                await StartGpsTimer();
                await LoadStopsCommand.ExecuteAsync(null);
            }
            else if (LocationDeniedPermanently)
            {
                if (AppInfo.Current.ShowSettingsUI != null)
                    AppInfo.Current.ShowSettingsUI();
            }
        }

        // ── Core permission logic ─────────────────────────────────────────
        private async Task<bool> RequestLocationPermissionAsync()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status == PermissionStatus.Granted)
            {
                status = await EnsureBackgroundLocationAsync();
            }

            if (status != PermissionStatus.Granted)
            {
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
            var speedKmh = speed.HasValue && speed.Value > 0.5 ? Math.Round(speed.Value) : 0;
            GpsStatus = $"{speedKmh} km/h";

            var sLat = lat.ToString(CultureInfo.InvariantCulture);
            var sLng = lng.ToString(CultureInfo.InvariantCulture);
            var sHdg = (heading ?? 0).ToString(CultureInfo.InvariantCulture);
            SendToMap?.Invoke($"moveBus({sLat}, {sLng}, {sHdg})");

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

        // ── Mark stop Reached (Step-by-step rule) ─────────────────────────
        [RelayCommand]
        private async Task MarkReachedAsync(DriverTripStop stop)
        {
            if (stop is null || stop.Status != "Pending") return;

            var idx = Stops.IndexOf(stop);
            if (idx > 0 && Stops[idx - 1].Status != "Departed")
            {
                SetError("Cannot reach this stop yet. All previous stops must be departed first in sequential order.");
                return;
            }

            await RunAsync(async () =>
            {
                var r = await _driverTrip.ReachStopAsync(TripId, stop.StopId);
                if (r.Success)
                {
                    stop.Status = "Reached";
                    stop.ReachedAt = DateTime.UtcNow;
                    UpdateMapStops();
                    await ShowToastAsync($"Reached '{stop.StopName}'");
                }
                else
                {
                    SetError(r.Message);
                }
            });
        }

        // ── Mark stop Departed (Step-by-step rule with student check) ──────
        [RelayCommand]
        private async Task MarkDepartedAsync(DriverTripStop stop)
        {
            if (stop is null || stop.Status != "Reached") return;

            var pendingStudents = stop.Students.Where(s => string.IsNullOrEmpty(s.BoardingStatus) || s.BoardingStatus.Equals("Pending", StringComparison.OrdinalIgnoreCase)).ToList();
            if (pendingStudents.Count > 0)
            {
                SetError($"Cannot depart this stop yet. Please update boarding status (Picked Up, No-Show, or On Leave) for all {pendingStudents.Count} assigned student(s) first.");
                return;
            }

            await RunAsync(async () =>
            {
                var r = await _driverTrip.DepartStopAsync(TripId, stop.StopId);
                if (r.Success)
                {
                    stop.Status = "Departed";
                    stop.DepartedAt = DateTime.UtcNow;
                    UpdateMapStops();
                    await ShowToastAsync($"Departed '{stop.StopName}'");
                }
                else
                {
                    SetError(r.Message);
                }
            });
        }

        // ── Update student boarding ───────────────────────────────────────
        [RelayCommand]
        private async Task ChangeStudentStatusAsync(DriverStudentStatus student)
        {
            if (student is null || string.IsNullOrEmpty(student.BoardingStatus)) return;

            var targetStop = Stops.FirstOrDefault(s => s.Students.Any(st => st.StudentId == student.StudentId));
            if (targetStop != null && targetStop.Status != "Reached")
            {
                SetError($"Student boarding status can only be updated when stop '{targetStop.StopName}' is Reached.");
                return;
            }

            await RunAsync(async () =>
            {
                int stopId = student.StopId > 0 ? student.StopId : (targetStop?.StopId ?? SelectedStop?.StopId ?? 0);

                var req = new UpdateBoardingRequest
                {
                    TripId = TripId,
                    StudentId = student.StudentId,
                    StopId = stopId,
                    BoardingStatus = student.BoardingStatus,
                    Status = student.BoardingStatus
                };

                var r = await _driverTrip.UpdateBoardingAsync(TripId, req);
                if (r.Success)
                {
                    student.NotifyStatusChanged();
                    await ShowToastAsync($"{student.StudentName}: {student.BoardingStatus}");
                }
                else
                {
                    SetError(r.Message);
                }
            });
        }

        [RelayCommand]
        private async Task UpdateBoardingAsync(
            (DriverStudentStatus student, string status) args)
        {
            var (student, status) = args;
            if (student is null || string.IsNullOrEmpty(status)) return;
            student.BoardingStatus = status;
            await ChangeStudentStatusAsync(student);
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
