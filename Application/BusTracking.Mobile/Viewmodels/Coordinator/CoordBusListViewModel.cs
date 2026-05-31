namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordBusListViewModel : BaseViewModel
    {
        private readonly IBusService _buses;

        [ObservableProperty] private ObservableCollection<BusItem> _items = [];
        [ObservableProperty] private string _searchText = "";

        public bool CanView => Can("bus.view");
        public bool CanAdd => Can("bus.add");
        public bool CanEdit => Can("bus.edit");
        public bool CanDelete => Can("bus.delete");

        public CoordBusListViewModel(IAuthService auth, INavigationService nav, IBusService buses)
            : base(auth, nav) { _buses = buses; Title = "Buses"; }

        public override async Task InitializeAsync() => await LoadAsync();
        public override async Task RefreshOnReturnAsync() => await LoadAsync();

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

        [RelayCommand]
        private Task AddAsync()
        {
            if (!CanAdd) return Task.CompletedTask;
            return Nav.GoToAsync("CoordBusForm");
        }

        [RelayCommand]
        private Task EditAsync(BusItem b)
        {
            if (!CanEdit) return Task.CompletedTask;
            return Nav.GoToAsync("CoordBusForm", new Dictionary<string, object> { ["BusId"] = b.BusId });
        }

        [RelayCommand]
        private Task ViewAsync(BusItem b)
        {
            if (!CanView) return Task.CompletedTask;
            return Nav.GoToAsync("CoordBusDetail", new Dictionary<string, object> { ["BusId"] = b.BusId });
        }

        [RelayCommand]
        private async Task DeleteAsync(BusItem b)
        {
            if (!CanDelete) return;
            if (!await ConfirmAsync("Delete Bus", $"Delete '{b.BusName}'?")) return;
            var r = await _buses.DeleteAsync(b.BusId);
            if (r.Success) { Items.Remove(b); await ShowToastAsync("Bus deleted."); }
            else SetError(r.Message);
        }
    }
}