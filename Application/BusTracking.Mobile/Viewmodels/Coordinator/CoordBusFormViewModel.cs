namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordBusFormViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IBusService _buses;
        private readonly IRouteService _routes;

        [ObservableProperty] private int? _busId;
        [ObservableProperty] private bool _isEditMode;
        [ObservableProperty] private string _busName = "";
        [ObservableProperty] private string _busNumber = "";
        [ObservableProperty] private int? _capacity;
        [ObservableProperty] private bool _isActive = true;
        [ObservableProperty] private List<RouteItem> _routeOptions = [];
        [ObservableProperty] private RouteItem? _selectedRoute;

        public CoordBusFormViewModel(IAuthService auth, INavigationService nav,
            IBusService buses, IRouteService routes)
            : base(auth, nav) { _buses = buses; _routes = routes; }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("BusId", out var id)) { BusId = (int)id; IsEditMode = true; Title = "Edit Bus"; }
            else Title = "Add Bus";
        }

        public override async Task InitializeAsync()
        {
            await RunAsync(async () =>
            {
                RouteOptions = await _routes.GetAllAsync();
                if (IsEditMode && BusId.HasValue)
                {
                    var bus = await _buses.GetByIdAsync(BusId.Value);
                    if (bus is null) return;
                    BusName = bus.BusName; BusNumber = bus.BusNumber;
                    Capacity = bus.Capacity; IsActive = bus.IsActive;
                    SelectedRoute = RouteOptions.FirstOrDefault(r => r.RouteId == bus.RouteId);
                }
            });
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(BusName) || string.IsNullOrWhiteSpace(BusNumber))
            { SetError("Bus name and number are required."); return; }

            await RunAsync(async () =>
            {
                ApiResponse<object> r = IsEditMode
                    ? await _buses.UpdateAsync(BusId!.Value, new UpdateBusRequest
                    { BusName = BusName, BusNumber = BusNumber, RouteId = SelectedRoute?.RouteId, Capacity = Capacity, IsActive = IsActive })
                    : await _buses.CreateAsync(new CreateBusRequest
                    { BusName = BusName, BusNumber = BusNumber, RouteId = SelectedRoute?.RouteId, Capacity = Capacity, IsActive = IsActive });

                if (r.Success) { await ShowToastAsync(IsEditMode ? "Bus updated." : "Bus created."); await Nav.GoBackAsync(); }
                else SetError(r.Message);
            });
        }

        [RelayCommand] private Task CancelAsync() => Nav.GoBackAsync();
    }
}
