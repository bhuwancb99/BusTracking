namespace BusTracking.Mobile.Viewmodels.Driver
{
    public partial class DriverTripListViewModel : BaseViewModel
    {
        private readonly IDriverTripService _driverTrip;

        [ObservableProperty] private ObservableCollection<DriverTripItem> _items = [];
        [ObservableProperty] private string _searchText = "";
        public string SearchPlaceholder => "Search trips…";
        public bool CanLoadMore => false;
        [RelayCommand] private async Task LoadMoreAsync() { }
        [RelayCommand] private async Task SearchAsync() => await RefreshAsync();
        [RelayCommand] private Task ViewAsync(DriverTripItem t) => OpenTripAsync(t);
        [ObservableProperty] private string _selectedDate = DateTime.Today.ToString("yyyy-MM-dd");

        public DriverTripListViewModel(IAuthService auth, INavigationService nav,
            IDriverTripService driverTrip) : base(auth, nav)
        {
            Title = "My Trips";
            _driverTrip = driverTrip;
        }

        public override Task InitializeAsync() => RefreshCommand.ExecuteAsync(null);

        public override Task RefreshOnReturnAsync() => RefreshCommand.ExecuteAsync(null);

        [RelayCommand]
        private async Task RefreshAsync()
        {
            IsRefreshing = true;
            try
            {
                await RunAsync(async () =>
                {
                    var list = await _driverTrip.GetMyTripsAsync(SelectedDate);
                    Items = new ObservableCollection<DriverTripItem>(list);
                    IsEmpty = Items.Count == 0;
                });
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        private async Task OpenTripAsync(DriverTripItem trip)
        {
            if (trip is null) return;

            var status = trip.Status?.Trim() ?? "";

            if (string.Equals(status, "InProgress", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(status, "In Progress", StringComparison.OrdinalIgnoreCase))
            {
                // Already started — go straight to tracking page
                await Nav.GoToAsync("DriverTracking",
                    new Dictionary<string, object> { ["TripId"] = trip.TripId });
                return;
            }

            if (string.Equals(status, "Scheduled", StringComparison.OrdinalIgnoreCase))
            {
                if (!await ConfirmAsync("Start Trip", $"Start the {trip.TripType} trip for {trip.TripDate}?"))
                    return;

                await RunAsync(async () =>
                {
                    var r = await _driverTrip.StartTripAsync(trip.TripId);
                    if (r.Success)
                    {
                        trip.Status = "InProgress";
                        await ShowToastAsync("Trip started.");
                        await Nav.GoToAsync("DriverTracking",
                            new Dictionary<string, object> { ["TripId"] = trip.TripId });
                    }
                    else
                    {
                        if (r.Message != null && r.Message.Contains("already", StringComparison.OrdinalIgnoreCase))
                        {
                            trip.Status = "InProgress";
                            await Nav.GoToAsync("DriverTracking",
                                new Dictionary<string, object> { ["TripId"] = trip.TripId });
                        }
                        else
                        {
                            SetError(r.Message);
                        }
                    }
                });
                return;
            }

            // Completed / Cancelled — open detail view (read-only)
            await Nav.GoToAsync("DriverTripDetail",
                new Dictionary<string, object> { ["TripId"] = trip.TripId });
        }

        // ── Driver watches live map for their own InProgress trip ─────────
        [RelayCommand]
        private Task TrackLiveAsync(DriverTripItem trip) =>
            Nav.GoToAsync("LiveTracking",
                new Dictionary<string, object> { ["TripId"] = trip.TripId });

        [RelayCommand]
        private async Task CancelTripAsync(DriverTripItem trip)
        {
            if (!await ConfirmAsync("Cancel Trip", "Cancel this trip? This cannot be undone."))
                return;
            await RunAsync(async () =>
            {
                var r = await _driverTrip.CancelTripAsync(trip.TripId);
                if (r.Success) { await ShowToastAsync("Trip cancelled."); await RefreshAsync(); }
                else SetError(r.Message);
            });
        }
    }
}