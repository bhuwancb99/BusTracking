namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminBusFormViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IBusService _buses;
        private readonly IRouteService _routes;
        private readonly IDriverService _drivers;
        private readonly IBusTypeService _busTypes;

        [ObservableProperty] private int? _busId;
        [ObservableProperty] private bool _isEditMode;
        [ObservableProperty] private string _busName = "";
        [ObservableProperty] private string _busNumber = "";
        [ObservableProperty] private int? _routeId;
        [ObservableProperty] private int? _driverUserId;
        [ObservableProperty] private int? _capacity;
        [ObservableProperty] private bool _isActive = true;

        [ObservableProperty] private List<BusTypeItem> _busTypeOptions = [];
        [ObservableProperty] private List<RouteItem> _routeOptions = [];
        [ObservableProperty] private List<DriverItem> _driverOptions = [];
        [ObservableProperty] private BusTypeItem? _selectedBusType;
        [ObservableProperty] private RouteItem? _selectedRoute;
        [ObservableProperty] private DriverItem? _selectedDriver;

        public AdminBusFormViewModel(IAuthService auth, INavigationService nav,
            IBusService buses, IRouteService routes, IDriverService drivers, IBusTypeService busTypes)
            : base(auth, nav)
        { _buses = buses; _routes = routes; _drivers = drivers; _busTypes = busTypes; }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("BusId", out var id))
            { BusId = (int)id; IsEditMode = true; Title = "Edit Bus"; }
            else Title = "Add Bus";
        }

        public override async Task InitializeAsync()
        {
            await RunAsync(async () =>
            {
                BusTypeOptions = await _busTypes.GetDropdownAsync();
                RouteOptions = await _routes.GetDropdownAsync();
                DriverOptions = await _drivers.GetAllForFormAsync();

                if (IsEditMode && BusId.HasValue)
                {
                    var bus = await _buses.GetByIdAsync(BusId.Value);
                    if (bus is null) return;
                    BusName = bus.BusName;
                    BusNumber = bus.BusNumber;
                    Capacity = bus.Capacity;
                    IsActive = bus.IsActive;
                    SelectedBusType = BusTypeOptions.FirstOrDefault(t => t.Id == bus.BusTypeId);
                    SelectedRoute = RouteOptions.FirstOrDefault(r => r.RouteId == bus.RouteId);
                    SelectedDriver = DriverOptions.FirstOrDefault(d => d.UserId == bus.DriverUserId);
                }
            });
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(BusName) || string.IsNullOrWhiteSpace(BusNumber))
            { SetError("Bus name and number are required."); return; }

            if (SelectedBusType is null)
            { SetError("Please select a bus type."); return; }

            await RunAsync(async () =>
            {
                var req = new UpdateBusRequest
                {
                    BusName = BusName,
                    BusNumber = BusNumber,
                    BusTypeId = SelectedBusType.Id,
                    RouteId = SelectedRoute?.RouteId,
                    Capacity = Capacity,
                    DriverUserId = SelectedDriver?.UserId,
                    IsActive = IsActive
                };

                ApiResponse<object> r = IsEditMode
                    ? await _buses.UpdateAsync(BusId!.Value, req)
                    : await _buses.CreateAsync(new CreateBusRequest
                    {
                        BusName = BusName,
                        BusNumber = BusNumber,
                        BusTypeId = SelectedBusType.Id,
                        RouteId = req.RouteId,
                        Capacity = Capacity,
                        DriverUserId = req.DriverUserId,
                        IsActive = IsActive
                    });

                if (r.Success)
                { await ShowToastAsync(IsEditMode ? "Bus updated." : "Bus created."); await Nav.GoBackAsync(); }
                else SetError(r.Message);
            });
        }

        [RelayCommand] private Task CancelAsync() => Nav.GoBackAsync();
    }
}