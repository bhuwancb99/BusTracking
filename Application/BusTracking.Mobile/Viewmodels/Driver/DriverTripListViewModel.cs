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

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await RunAsync(async () =>
            {
                var list = await _driverTrip.GetMyTripsAsync(SelectedDate);
                Items = new ObservableCollection<DriverTripItem>(list);
                IsEmpty = Items.Count == 0;
            });
        }

        [RelayCommand]
        private async Task OpenTripAsync(DriverTripItem trip)
        {
            if (trip.Status == "InProgress")
            {
                // Go straight to live tracking
                await Nav.GoToAsync("DriverTracking",
                    new Dictionary<string, object> { ["TripId"] = trip.TripId });
            }
            else
            {
                // Show detail / start options
                await Nav.GoToAsync("DriverTripDetail",
                    new Dictionary<string, object> { ["TripId"] = trip.TripId });
            }
        }

        [RelayCommand]
        private async Task StartTripAsync(DriverTripItem trip)
        {
            if (!await ConfirmAsync("Start Trip", $"Start the {trip.TripType} trip for {trip.TripDate}?"))
                return;
            await RunAsync(async () =>
            {
                var r = await _driverTrip.StartTripAsync(trip.TripId);
                if (r.Success)
                {
                    await ShowToastAsync("Trip started.");
                    await Nav.GoToAsync("DriverTracking",
                        new Dictionary<string, object> { ["TripId"] = trip.TripId });
                }
                else SetError(r.Message);
            });
        }

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