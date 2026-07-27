namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordBusFormViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IBusService _buses;
        private readonly IRouteService _routes;
        private readonly IDriverService _drivers;
        private readonly IBusTypeService _busTypes;

        [ObservableProperty] private int? _busId;
        [ObservableProperty] private bool _isEditMode;
        [ObservableProperty] private string _busName = "";
        [ObservableProperty] private string _busNumber = "";
        [ObservableProperty] private int? _capacity;
        [ObservableProperty] private bool _isActive = true;

        // Compliance & Maintenance Tracking
        [ObservableProperty] private DateTime? _insuranceExpiryDate;
        [ObservableProperty] private DateTime? _fitnessExpiryDate;
        [ObservableProperty] private DateTime? _pucExpiryDate;
        [ObservableProperty] private DateTime? _lastServiceDate;

        [ObservableProperty] private bool _isInsuranceCalendarOpen;
        [ObservableProperty] private bool _isFitnessCalendarOpen;
        [ObservableProperty] private bool _isPucCalendarOpen;
        [ObservableProperty] private bool _isServiceCalendarOpen;

        [RelayCommand] private void OpenInsuranceCalendar() => IsInsuranceCalendarOpen = true;
        [RelayCommand] private void OpenFitnessCalendar() => IsFitnessCalendarOpen = true;
        [RelayCommand] private void OpenPucCalendar() => IsPucCalendarOpen = true;
        [RelayCommand] private void OpenServiceCalendar() => IsServiceCalendarOpen = true;

        [ObservableProperty] private List<BusTypeItem> _busTypeOptions = [];
        [ObservableProperty] private ObservableCollection<SelectableItem> _selectableRoutes = [];
        [ObservableProperty] private ObservableCollection<SelectableItem> _selectableDrivers = [];
        [ObservableProperty] private BusTypeItem? _selectedBusType;

        public CoordBusFormViewModel(IAuthService auth, INavigationService nav,
            IBusService buses, IRouteService routes, IDriverService drivers, IBusTypeService busTypes)
            : base(auth, nav)
        { _buses = buses; _routes = routes; _drivers = drivers; _busTypes = busTypes; }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("BusId", out var id)) { BusId = (int)id; IsEditMode = true; Title = "Edit Bus"; }
            else Title = "Add Bus";
        }

        public override async Task InitializeAsync()
        {
            await RunAsync(async () =>
            {
                BusTypeOptions = await _busTypes.GetDropdownAsync();
                var routes = await _routes.GetDropdownAsync();
                var drivers = await _drivers.GetAllForFormAsync();

                SelectableRoutes = new ObservableCollection<SelectableItem>(
                    routes.Select(r => new SelectableItem { Id = r.RouteId, Name = r.RouteName, Code = r.RouteCode }));

                SelectableDrivers = new ObservableCollection<SelectableItem>(
                    drivers.Select(d => new SelectableItem { Id = d.UserId, Name = d.FullName, Code = d.UserName }));

                if (IsEditMode && BusId.HasValue)
                {
                    var bus = await _buses.GetByIdAsync(BusId.Value);
                    if (bus is null) return;
                    BusName = bus.BusName; BusNumber = bus.BusNumber;
                    Capacity = bus.Capacity; IsActive = bus.IsActive;

                    if (DateTime.TryParse(bus.InsuranceExpiryDate, out var ins)) InsuranceExpiryDate = ins;
                    if (DateTime.TryParse(bus.FitnessExpiryDate, out var fit)) FitnessExpiryDate = fit;
                    if (DateTime.TryParse(bus.PucExpiryDate, out var puc)) PucExpiryDate = puc;
                    if (DateTime.TryParse(bus.LastServiceDate, out var srv)) LastServiceDate = srv;

                    SelectedBusType = BusTypeOptions.FirstOrDefault(t => t.Id == bus.BusTypeId);

                    var activeRouteIds = bus.RouteIds.Count > 0 ? bus.RouteIds : (bus.RouteId.HasValue ? [bus.RouteId.Value] : new List<int>());
                    foreach (var sr in SelectableRoutes)
                    {
                        if (activeRouteIds.Contains(sr.Id)) sr.IsSelected = true;
                    }

                    var activeDriverIds = bus.DriverUserIds.Count > 0 ? bus.DriverUserIds : (bus.DriverUserId.HasValue ? [bus.DriverUserId.Value] : new List<int>());
                    foreach (var sd in SelectableDrivers)
                    {
                        if (activeDriverIds.Contains(sd.Id)) sd.IsSelected = true;
                    }
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

            var selectedRouteIds = SelectableRoutes.Where(r => r.IsSelected).Select(r => r.Id).ToList();
            var selectedDriverIds = SelectableDrivers.Where(d => d.IsSelected).Select(d => d.Id).ToList();

            await RunAsync(async () =>
            {
                var req = new UpdateBusRequest
                {
                    BusName = BusName,
                    BusNumber = BusNumber,
                    BusTypeId = SelectedBusType.Id,
                    RouteId = selectedRouteIds.FirstOrDefault(),
                    RouteIds = selectedRouteIds,
                    Capacity = Capacity,
                    DriverUserId = selectedDriverIds.FirstOrDefault(),
                    DriverUserIds = selectedDriverIds,
                    InsuranceExpiryDate = InsuranceExpiryDate?.ToString("yyyy-MM-dd"),
                    FitnessExpiryDate = FitnessExpiryDate?.ToString("yyyy-MM-dd"),
                    PucExpiryDate = PucExpiryDate?.ToString("yyyy-MM-dd"),
                    LastServiceDate = LastServiceDate?.ToString("yyyy-MM-dd"),
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
                        RouteIds = selectedRouteIds,
                        Capacity = Capacity,
                        DriverUserId = req.DriverUserId,
                        DriverUserIds = selectedDriverIds,
                        InsuranceExpiryDate = req.InsuranceExpiryDate,
                        FitnessExpiryDate = req.FitnessExpiryDate,
                        PucExpiryDate = req.PucExpiryDate,
                        LastServiceDate = req.LastServiceDate,
                        IsActive = IsActive
                    });

                if (r.Success) { await ShowToastAsync(IsEditMode ? "Bus updated." : "Bus created."); await Nav.GoBackAsync(); }
                else SetError(r.Message);
            });
        }

        [RelayCommand] private Task CancelAsync() => Nav.GoBackAsync();
    }
}
