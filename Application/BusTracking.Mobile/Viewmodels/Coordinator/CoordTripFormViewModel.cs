namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordTripFormViewModel : BaseViewModel
    {
        private readonly ITripService _trips;
        private readonly IBusService _buses;
        private readonly IRouteService _routes;

        [ObservableProperty] private List<BusItem> _busOptions = [];
        [ObservableProperty] private List<RouteItem> _routeOptions = [];
        [ObservableProperty] private BusItem? _selectedBus;
        [ObservableProperty] private RouteItem? _selectedRoute;
        [ObservableProperty] private string _tripType = "Morning";
        [ObservableProperty] private DateTime _tripDate = DateTime.Today;

        public List<string> TripTypes => ["Morning", "Evening"];

        public CoordTripFormViewModel(IAuthService auth, INavigationService nav,
            ITripService trips, IBusService buses, IRouteService routes)
            : base(auth, nav) { _trips = trips; _buses = buses; _routes = routes; Title = "Create Trip"; }

        public override async Task InitializeAsync()
        {
            await RunAsync(async () =>
            {
                BusOptions = await _buses.GetAllForFormAsync();
                RouteOptions = await _routes.GetDropdownAsync();
            });
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (SelectedBus is null || SelectedRoute is null)
            { SetError("Please select a bus and route."); return; }

            await RunAsync(async () =>
            {
                var r = await _trips.CreateAsync(new CreateTripRequest
                {
                    BusId = SelectedBus.BusId,
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
