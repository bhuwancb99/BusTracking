namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminTripFormViewModel : BaseViewModel
    {
        private readonly ITripService _trips;
        private readonly IBusService _buses;
        private readonly IRouteService _routes;
        private readonly IDriverService _drivers;

        [ObservableProperty] private List<BusItem> _busOptions = [];
        [ObservableProperty] private List<DriverItem> _driverOptions = [];
        [ObservableProperty] private List<RouteItem> _routeOptions = [];

        [ObservableProperty] private BusItem? _selectedBus;
        [ObservableProperty] private DriverItem? _selectedDriver;
        [ObservableProperty] private RouteItem? _selectedRoute;

        [ObservableProperty] private string? _tripType;
        [ObservableProperty] private DateTime _tripDate = DateTime.Today;

        public List<string> TripTypes => ["Morning", "Evening", "SpecialEvent", "ExamRoute"];

        public AdminTripFormViewModel(IAuthService auth, INavigationService nav,
            ITripService trips, IBusService buses, IRouteService routes, IDriverService drivers)
            : base(auth, nav)
        { _trips = trips; _buses = buses; _routes = routes; _drivers = drivers; Title = "Add Trip"; }

        public override async Task InitializeAsync()
        {
            await RunAsync(async () =>
            {
                BusOptions = await _buses.GetAllForFormAsync();
                var allDrivers = await _drivers.GetAllAsync();
                DriverOptions = allDrivers.Items;
                RouteOptions = await _routes.GetDropdownAsync();
            });
        }

        [ObservableProperty] private List<StopItem> _stopsPreview = [];

        partial void OnSelectedBusChanged(BusItem? value)
        {
            SelectedDriver = null;
            SelectedRoute = null;
            StopsPreview = [];

            if (value != null && DriverOptions.Count > 0)
            {
                var matchedDriver = DriverOptions.FirstOrDefault(d => d.BusId == value.BusId || (d.BusName != null && d.BusName.Contains(value.BusName)));
                if (matchedDriver != null)
                {
                    SelectedDriver = matchedDriver;
                }
            }
        }

        partial void OnSelectedDriverChanged(DriverItem? value)
        {
            SelectedRoute = null;
            StopsPreview = [];

            if (value != null && SelectedBus != null && SelectedBus.RouteId.HasValue && RouteOptions.Count > 0)
            {
                SelectedRoute = RouteOptions.FirstOrDefault(r => r.RouteId == SelectedBus.RouteId);
            }
        }

        partial void OnSelectedRouteChanged(RouteItem? value)
        {
            StopsPreview = [];
            if (value != null)
            {
                var routeId = value.RouteId;
                _ = Task.Run(async () =>
                {
                    var stops = await _routes.GetStopsAsync(routeId);
                    MainThread.BeginInvokeOnMainThread(() => StopsPreview = stops);
                });
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (SelectedBus is null)
            { SetError("Please select a bus."); return; }
            if (SelectedDriver is null)
            { SetError("Please select a driver."); return; }
            if (SelectedRoute is null)
            { SetError("Please select a route."); return; }
            if (string.IsNullOrEmpty(TripType))
            { SetError("Please select a trip type."); return; }

            await RunAsync(async () =>
            {
                var r = await _trips.CreateAsync(new CreateTripRequest
                {
                    BusId = SelectedBus.BusId,
                    DriverId = SelectedDriver.UserId,
                    RouteId = SelectedRoute.RouteId,
                    TripType = TripType,
                    TripDate = TripDate
                });
                if (r.Success) { await ShowToastAsync("Trip created."); await Nav.GoBackAsync(); }
                else SetError(r.Message);
            });
        }

        [RelayCommand] private Task CancelAsync() => Nav.GoBackAsync();
    }
}
