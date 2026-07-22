namespace BusTracking.Mobile.Viewmodels.Driver
{
    public partial class DriverTripDetailViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IDriverTripService _driverTrips;

        [ObservableProperty] private int _tripId;
        [ObservableProperty] private DriverTripItem? _trip;
        [ObservableProperty] private ObservableCollection<DriverTripStop> _stops = [];

        public bool IsTripCompleted => Trip?.Status?.Equals("Completed", StringComparison.OrdinalIgnoreCase) == true || Trip?.Status?.Equals("Cancelled", StringComparison.OrdinalIgnoreCase) == true;
        public bool IsTripInProgress => Trip?.Status?.Equals("In Progress", StringComparison.OrdinalIgnoreCase) == true;
        public bool CanStartTrip => !IsTripCompleted && !IsTripInProgress;
        public bool CanCancelTrip => CanStartTrip;
        public bool CanEndTrip => IsTripInProgress;
        public bool HasActionButtons => !IsTripCompleted;

        public Color StatusBadgeColor => Trip?.Status switch
        {
            "Completed" => Color.FromArgb("#16a34a"),
            "In Progress" => Color.FromArgb("#2563eb"),
            "Cancelled" => Color.FromArgb("#dc2626"),
            _ => Color.FromArgb("#f59e0b")
        };

        public DriverTripDetailViewModel(IAuthService auth, INavigationService nav, IDriverTripService driverTrips)
            : base(auth, nav)
        {
            _driverTrips = driverTrips;
            Title = "Trip Details";
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("TripId", out var id)) TripId = (int)id;
        }

        public override async Task InitializeAsync() => await LoadAsync();

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                var trips = await _driverTrips.GetMyTripsAsync();
                Trip = trips.FirstOrDefault(t => t.TripId == TripId);
                var stopsList = await _driverTrips.GetTripStopsAsync(TripId);
                var studentsList = await _driverTrips.GetTripStudentsAsync(TripId);

                foreach (var stop in stopsList)
                {
                    stop.Students = studentsList
                        .Where(st => st.StopOrder == stop.StopOrder ||
                                     (st.StopName != null && st.StopName.Equals(stop.StopName, StringComparison.OrdinalIgnoreCase)))
                        .ToList();
                }

                Stops = new ObservableCollection<DriverTripStop>(stopsList);

                NotifyStatusProperties();
            });
        }

        private void NotifyStatusProperties()
        {
            OnPropertyChanged(nameof(IsTripCompleted));
            OnPropertyChanged(nameof(IsTripInProgress));
            OnPropertyChanged(nameof(CanStartTrip));
            OnPropertyChanged(nameof(CanCancelTrip));
            OnPropertyChanged(nameof(CanEndTrip));
            OnPropertyChanged(nameof(HasActionButtons));
            OnPropertyChanged(nameof(StatusBadgeColor));
        }

        [RelayCommand]
        private async Task StartTripAsync()
        {
            if (!await ConfirmAsync("Start Trip", "Start this trip now?")) return;
            var r = await _driverTrips.StartTripAsync(TripId);
            if (r.Success)
            {
                await ShowToastAsync("Trip started.");
                await Nav.GoToAsync("DriverTracking", new Dictionary<string, object> { ["TripId"] = TripId });
            }
            else SetError(r.Message);
        }

        [RelayCommand]
        private async Task GoToLiveTrackingAsync()
        {
            await Nav.GoToAsync("DriverTracking", new Dictionary<string, object> { ["TripId"] = TripId });
        }

        [RelayCommand]
        private async Task EndTripAsync()
        {
            if (!await ConfirmAsync("End Trip", "End this trip?")) return;
            var r = await _driverTrips.EndTripAsync(TripId);
            if (r.Success)
            {
                await ShowToastAsync("Trip completed.");
                await LoadAsync();
            }
            else SetError(r.Message);
        }

        [RelayCommand]
        private async Task CancelTripAsync()
        {
            if (!await ConfirmAsync("Cancel Trip", "Cancel this trip?")) return;
            var r = await _driverTrips.CancelTripAsync(TripId);
            if (r.Success)
            {
                await ShowToastAsync("Trip cancelled.");
                await LoadAsync();
            }
            else SetError(r.Message);
        }

        [RelayCommand]
        private async Task GoBackAsync() => await Nav.GoBackAsync();
    }
}