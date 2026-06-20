namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordBusListViewModel : BaseViewModel
    {
        private readonly IBusService _buses;

        [ObservableProperty] private ObservableCollection<BusItem> _items = [];
        [ObservableProperty] private string _searchText = "";
        [ObservableProperty] private string _selectedFilter = "Active";   // Active | Inactive | Both
        [ObservableProperty] private int _currentPage = 1;
        [ObservableProperty] private bool _canLoadMore;

        public string SearchPlaceholder => "Search buses…";
        public List<string> FilterOptions => ["Active", "Inactive", "Both"];
        public bool CanAdd => Can("bus.add");
        public bool CanEdit => Can("bus.edit");
        public bool CanDelete => Can("bus.delete");

        public CoordBusListViewModel(IAuthService auth, INavigationService nav, IBusService buses)
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
                    SearchText.Trim().Length > 0 ? SearchText.Trim() : null, CurrentPage, SelectedFilter);
                Items = new ObservableCollection<BusItem>(data.Items);
                IsEmpty = !Items.Any();
                CanLoadMore = data.PageNumber < data.TotalPages;
            });
        }

        [RelayCommand]
        private async Task LoadMoreAsync()
        {
            if (!CanLoadMore || IsBusy) return;
            await RunAsync(async () =>
            {
                CurrentPage++;
                var data = await _buses.GetAllAsync(
                    SearchText.Trim().Length > 0 ? SearchText.Trim() : null, CurrentPage, SelectedFilter);
                foreach (var item in data.Items) Items.Add(item);
                CanLoadMore = data.PageNumber < data.TotalPages;
            });
        }

        [RelayCommand] private async Task SearchAsync() => await LoadAsync();
        [RelayCommand] private Task AddAsync() => Nav.GoToAsync("CoordBusForm");
        [RelayCommand]
        private Task EditAsync(BusItem b) =>
            Nav.GoToAsync("CoordBusForm", new Dictionary<string, object> { ["BusId"] = b.BusId });
        [RelayCommand]
        private Task DetailAsync(BusItem b) =>
            Nav.GoToAsync("CoordBusDetail", new Dictionary<string, object> { ["BusId"] = b.BusId });

        [RelayCommand]
        private void Filter(string filter) => SelectedFilter = filter;
    }
}
