namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordFuelLogListViewModel : BaseViewModel
    {
        private readonly IFuelLogService _fuelLogService;

        [ObservableProperty] private ObservableCollection<FuelLogItem> _items = [];

        public bool CanAdd => Can("fuellog.manage") || Can("fuellog.view");
        public bool CanEdit => Can("fuellog.manage") || Can("fuellog.view");

        public CoordFuelLogListViewModel(IAuthService auth, INavigationService nav, IFuelLogService fuelLogService)
            : base(auth, nav)
        {
            _fuelLogService = fuelLogService;
            Title = "Fuel & Mileage Logs";
        }

        public override async Task InitializeAsync() => await LoadAsync();
        public override async Task RefreshOnReturnAsync() => await LoadAsync();

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                var response = await _fuelLogService.GetAllAsync();
                if (response.Success && response.Data != null)
                {
                    Items = new ObservableCollection<FuelLogItem>(response.Data);
                }
                else
                {
                    Items = [];
                }
                IsEmpty = !Items.Any();
            });
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            IsRefreshing = true;
            try
            {
                await LoadAsync();
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        private Task AddAsync() => Nav.GoToAsync("CoordFuelLogForm");

        [RelayCommand]
        private Task EditAsync(FuelLogItem item)
        {
            if (item == null) return Task.CompletedTask;
            return Nav.GoToAsync("CoordFuelLogForm", new Dictionary<string, object> { { "FuelLogItem", item } });
        }
    }
}
