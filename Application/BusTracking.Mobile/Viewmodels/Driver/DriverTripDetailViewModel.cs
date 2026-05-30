namespace BusTracking.Mobile.Viewmodels.Driver
{
    public partial class DriverTripDetailViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IDriverTripService _driverTrips;

        [ObservableProperty] private int _tripId;
        [ObservableProperty] private DriverTripItem? _trip;
        [ObservableProperty] private ObservableCollection<DriverTripStop> _stops = [];

        public DriverTripDetailViewModel(IAuthService auth, INavigationService nav, IDriverTripService driverTrips)
            : base(auth, nav) { _driverTrips = driverTrips; Title = "Trip Details"; }

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
                var stops = await _driverTrips.GetTripStopsAsync(TripId);
                Stops = new ObservableCollection<DriverTripStop>(stops);
            });
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
        private async Task EndTripAsync()
        {
            if (!await ConfirmAsync("End Trip", "End this trip?")) return;
            var r = await _driverTrips.EndTripAsync(TripId);
            if (r.Success) { await ShowToastAsync("Trip completed."); await LoadAsync(); }
            else SetError(r.Message);
        }

        [RelayCommand]
        private async Task CancelTripAsync()
        {
            if (!await ConfirmAsync("Cancel Trip", "Cancel this trip?")) return;
            var r = await _driverTrips.CancelTripAsync(TripId);
            if (r.Success) { await ShowToastAsync("Trip cancelled."); await LoadAsync(); }
            else SetError(r.Message);
        }

        /// <summary>
        /// Cycles a student's boarding status:
        ///   Pending → PickedUp → NoShow → Pending
        /// Sends StopId + BoardingStatus to the API (matches UpdateBoardingRequest model).
        /// Bound to each student row's tap/button in the XAML via CommandParameter=student.
        /// </summary>
        [RelayCommand]
        private async Task ToggleBoardingAsync(DriverStudentStatus student)
        {
            // Determine the NEXT status in the cycle
            var nextStatus = student.BoardingStatus switch
            {
                "Pending" => "PickedUp",
                "PickedUp" => "NoShow",
                _ => "Pending"     // NoShow | anything else → back to Pending
            };

            // Find the stop this student belongs to
            var parentStop = Stops.FirstOrDefault(s => s.Students.Contains(student));
            if (parentStop is null) return;

            var req = new UpdateBoardingRequest
            {
                StudentId = student.StudentId,
                StopId = parentStop.StopId,
                BoardingStatus = nextStatus
            };

            var r = await _driverTrips.UpdateBoardingAsync(TripId, req);
            if (r.Success)
            {
                // Update locally so the UI reflects immediately without a full reload
                student.BoardingStatus = nextStatus;
                // Force CollectionView to re-evaluate color/icon by rebuilding collection
                await LoadAsync();
            }
            else SetError(r.Message);
        }
    }
}