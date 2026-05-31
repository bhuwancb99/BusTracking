namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminBusListViewModel : BaseViewModel
    {
        private readonly IBusService _buses;

        [ObservableProperty] private ObservableCollection<BusItem> _items = [];
        [ObservableProperty] private string _searchText = "";
        [ObservableProperty] private int _currentPage = 1;
        [ObservableProperty] private bool _canLoadMore;
        [ObservableProperty] private string _selectedFilter = "Active";   // Active | Inactive | Both

        public string SearchPlaceholder => "Search buses…";
        public List<string> FilterOptions => ["Active", "Inactive", "Both"];

        // null = both, true = active, false = inactive
        private bool? ActiveFilter => SelectedFilter switch
        {
            "Active" => true,
            "Inactive" => false,
            _ => null
        };

        public bool CanAdd => Can("bus.add");
        public bool CanEdit => Can("bus.edit");
        public bool CanDelete => Can("bus.delete");

        public AdminBusListViewModel(IAuthService auth, INavigationService nav, IBusService buses)
            : base(auth, nav) { _buses = buses; Title = "Buses"; }

        public override async Task InitializeAsync() => await LoadAsync();
        public override async Task RefreshOnReturnAsync() => await LoadAsync();

        // Re-load when filter chip changes
        partial void OnSelectedFilterChanged(string value) => LoadCommand.ExecuteAsync(null);

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                CurrentPage = 1;
                var data = await _buses.GetAllAsync(
                    SearchText.Trim().Length > 0 ? SearchText : null, 1, ActiveFilter);
                Items = new ObservableCollection<BusItem>(data);
                IsEmpty = !Items.Any();
                CanLoadMore = data.Count == 20;
            });
        }

        [RelayCommand]
        private async Task LoadMoreAsync()
        {
            if (!CanLoadMore || IsBusy) return;
            await RunAsync(async () =>
            {
                CurrentPage++;
                var data = await _buses.GetAllAsync(SearchText, CurrentPage, ActiveFilter);
                foreach (var item in data) Items.Add(item);
                CanLoadMore = data.Count == 20;
            });
        }

        [RelayCommand] private async Task SearchAsync() => await LoadAsync();
        [RelayCommand] private Task AddAsync() => Nav.GoToAsync("AdminBusForm");

        // Tap row → Detail page
        [RelayCommand]
        private Task TapAsync(BusItem bus) =>
            Nav.GoToAsync("AdminBusDetail", new Dictionary<string, object> { ["BusId"] = bus.BusId });

        [RelayCommand]
        private Task EditAsync(BusItem bus) =>
            Nav.GoToAsync("AdminBusForm", new Dictionary<string, object> { ["BusId"] = bus.BusId });

        [RelayCommand]
        private async Task ToggleAsync(BusItem bus)
        {
            var r = await _buses.ToggleAsync(bus.BusId);
            if (r.Success) { await ShowToastAsync(r.Message); await LoadAsync(); }
            else SetError(r.Message);
        }

        // Only active buses can be deleted
        [RelayCommand]
        private async Task DeleteAsync(BusItem bus)
        {
            if (!bus.IsActive) return;
            if (!await ConfirmAsync("Delete Bus", $"Delete '{bus.BusName}'?")) return;
            var r = await _buses.DeleteAsync(bus.BusId);
            if (r.Success) { Items.Remove(bus); await ShowToastAsync("Bus deleted."); }
            else SetError(r.Message);
        }

        [RelayCommand]
        private void Filter(string filter) => SelectedFilter = filter;
    }
}