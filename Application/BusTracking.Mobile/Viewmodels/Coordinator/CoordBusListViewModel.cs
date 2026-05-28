namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordBusListViewModel : BaseViewModel
    {
        private readonly IBusService _buses;

        [ObservableProperty] private ObservableCollection<BusItem> _items = [];
        [ObservableProperty] private string _searchText = "";

        public bool CanAdd => Can("bus.add");
        public bool CanEdit => Can("bus.edit");
        public bool CanDelete => Can("bus.delete");
        public bool CanView => Can("bus.view");

        public CoordBusListViewModel(IAuthService auth, INavigationService nav, IBusService buses)
            : base(auth, nav) { _buses = buses; Title = "Buses"; }

        public override async Task InitializeAsync() => await LoadAsync();

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                var data = await _buses.GetAllAsync(SearchText.Trim().Length > 0 ? SearchText : null);
                Items = new ObservableCollection<BusItem>(data);
                IsEmpty = !Items.Any();
            });
        }

        [RelayCommand] private async Task SearchAsync() => await LoadAsync();
        [RelayCommand] private Task AddAsync() => Nav.GoToAsync("CoordBusForm");
        [RelayCommand]
        private Task EditAsync(BusItem b) =>
            Nav.GoToAsync("CoordBusForm", new Dictionary<string, object> { ["BusId"] = b.BusId });
        [RelayCommand]
        private Task ViewAsync(BusItem b) =>
            Nav.GoToAsync("CoordBusDetail", new Dictionary<string, object> { ["BusId"] = b.BusId });
    }
}
