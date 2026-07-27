namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminFuelLogFormViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IFuelLogService _fuelLogService;
        private readonly IBusService _busService;
        private int? _editBusId;

        [ObservableProperty] private int _fuelLogId;
        [ObservableProperty] private bool _isEditMode;
        [ObservableProperty] private decimal _odometerReading;
        [ObservableProperty] private decimal _fuelLiters;
        [ObservableProperty] private decimal _totalCost;
        [ObservableProperty] private DateTime _fuelDate = DateTime.Today;
        [ObservableProperty] private string? _notes;

        [ObservableProperty] private bool _isFuelDateCalendarOpen;
        [RelayCommand] private void OpenFuelDateCalendar() => IsFuelDateCalendarOpen = true;

        [ObservableProperty] private List<BusItem> _busOptions = [];
        [ObservableProperty] private BusItem? _selectedBus;

        public AdminFuelLogFormViewModel(IAuthService auth, INavigationService nav, IFuelLogService fuelLogService, IBusService busService)
            : base(auth, nav)
        {
            _fuelLogService = fuelLogService;
            _busService = busService;
            Title = "Record Fuel Fill";
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("FuelLogItem", out var val) && val is FuelLogItem item)
            {
                FuelLogId = item.FuelLogId;
                IsEditMode = true;
                Title = "Edit Fuel Log";
                _editBusId = item.BusId;
                OdometerReading = item.OdometerReading;
                FuelLiters = item.FuelLiters;
                TotalCost = item.TotalCost;
                if (DateTime.TryParse(item.FuelDate, out var dt)) FuelDate = dt;
                Notes = item.Notes;
            }
            else
            {
                Title = "Record Fuel Fill";
            }
        }

        public override async Task InitializeAsync()
        {
            await RunAsync(async () =>
            {
                var busResult = await _busService.GetAllAsync(null, 1, "Active");
                if (busResult != null && busResult.Items != null)
                {
                    BusOptions = busResult.Items;
                    if (IsEditMode && _editBusId.HasValue)
                    {
                        SelectedBus = BusOptions.FirstOrDefault(b => b.BusId == _editBusId.Value);
                    }
                }
            });
        }

        [RelayCommand]
        private Task CancelAsync() => Nav.GoBackAsync();

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (SelectedBus == null)
            {
                SetError("Please select a bus.");
                return;
            }
            if (FuelLiters <= 0)
            {
                SetError("Please enter valid fuel liters.");
                return;
            }

            await RunAsync(async () =>
            {
                var item = new FuelLogItem
                {
                    FuelLogId = FuelLogId,
                    BusId = SelectedBus.BusId,
                    OdometerReading = OdometerReading,
                    FuelLiters = FuelLiters,
                    TotalCost = TotalCost,
                    FuelDate = FuelDate.ToString("yyyy-MM-dd"),
                    Notes = Notes
                };

                var res = IsEditMode
                    ? await _fuelLogService.UpdateAsync(item)
                    : await _fuelLogService.CreateAsync(item);

                if (res.Success)
                {
                    await ShowToastAsync(IsEditMode ? "Fuel log updated successfully." : "Fuel log recorded successfully.");
                    await Nav.GoBackAsync();
                }
                else
                {
                    SetError(res.Message ?? "Failed to save fuel log.");
                }
            });
        }
    }
}
