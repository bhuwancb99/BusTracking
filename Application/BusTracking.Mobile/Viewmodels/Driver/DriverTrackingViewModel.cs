namespace BusTracking.Mobile.Viewmodels.Driver
{
    [QueryProperty(nameof(TripId), "TripId")]
    public partial class DriverTrackingViewModel : BaseViewModel
    {
        private readonly IDriverTripService _driverTrip;
        private IDispatcherTimer? _gpsTimer;

        [ObservableProperty] private int _tripId;
        [ObservableProperty] private string _tripStatus = "Loading…";
        [ObservableProperty] private string _tripTypeLabel = "";
        [ObservableProperty] private bool _isInProgress;
        [ObservableProperty] private ObservableCollection<DriverTripStop> _stops = [];
        [ObservableProperty] private DriverTripStop? _selectedStop;

        public DriverTrackingViewModel(IAuthService auth, INavigationService nav,
            IDriverTripService driverTrip) : base(auth, nav)
        {
            Title = "Live Tracking";
            _driverTrip = driverTrip;
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

                // Auto-select the first non-departed stop
                SelectedStop = Stops.FirstOrDefault(s => s.Status != "Departed") ?? Stops.FirstOrDefault();
            });
        }

        // ── Mark stop Reached ─────────────────────────────────────────────
        [RelayCommand]
        private async Task MarkReachedAsync(DriverTripStop stop)
        {
            if (stop.Status != "Pending") return;
            await RunAsync(async () =>
            {
                // Optimistic update
                stop.Status = "Reached";
                OnPropertyChanged(nameof(Stops));
                await LoadStopsAsync(); // refresh from server
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
        private async Task UpdateBoardingAsync((DriverStudentStatus student, string status) args)
        {
            var (student, status) = args;
            await RunAsync(async () =>
            {
                if (SelectedStop is null) return;
                var req = new UpdateBoardingRequest
                {
                    StudentId = student.StudentId,
                    StopId = SelectedStop.StopId,
                    BoardingStatus = status
                };
                var r = await _driverTrip.UpdateBoardingAsync(TripId, req);
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
            if (!await ConfirmAsync("End Trip", "End this trip? All tracking will stop."))
                return;
            await RunAsync(async () =>
            {
                StopGpsTimer();
                var r = await _driverTrip.EndTripAsync(TripId);
                if (r.Success)
                {
                    await ShowToastAsync("Trip ended successfully.");
                    await Nav.GoToAsync("//DriverDashboard");
                }
                else SetError(r.Message);
            });
        }

        // ── GPS pinging ───────────────────────────────────────────────────
        public void StartGpsTimer()
        {
            _gpsTimer = Application.Current?.Dispatcher.CreateTimer();
            if (_gpsTimer is null) return;
            _gpsTimer.Interval = TimeSpan.FromSeconds(10);
            _gpsTimer.Tick += async (_, _) => await PingGpsAsync();
            _gpsTimer.Start();
        }

        public void StopGpsTimer() => _gpsTimer?.Stop();

        private async Task PingGpsAsync()
        {
            try
            {
                var loc = await Geolocation.GetLocationAsync(new GeolocationRequest(
                    GeolocationAccuracy.Best, TimeSpan.FromSeconds(5)));
                if (loc is null) return;
                await _driverTrip.PingLocationAsync(TripId, new LocationPingRequest
                {
                    Latitude = loc.Latitude,
                    Longitude = loc.Longitude,
                    Speed = loc.Speed,
                    Heading = loc.Course
                });
            }
            catch
            {
                // GPS errors are non-fatal — silently ignore
            }
        }

        [RelayCommand]
        private Task GoBackAsync() => Nav.GoBackAsync();
    }
}