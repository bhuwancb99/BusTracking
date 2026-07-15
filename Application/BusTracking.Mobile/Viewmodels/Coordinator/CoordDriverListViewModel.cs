namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordDriverListViewModel : BaseViewModel
    {
        private readonly IDriverService _drivers;

        [ObservableProperty] private ObservableCollection<DriverItem> _items = [];
        [ObservableProperty] private string _searchText = "";
        [ObservableProperty] private string _selectedFilter = "Active";   // Active | Inactive | Both
        [ObservableProperty] private int _currentPage = 1;
        [ObservableProperty] private bool _canLoadMore;

        public string SearchPlaceholder => "Search drivers…";
        public List<string> FilterOptions => ["Active", "Inactive", "Both"];
        public bool CanAdd => Can("driver.add");
        public bool CanEdit => Can("driver.edit");
        public bool CanDelete => Can("driver.delete");

        public CoordDriverListViewModel(IAuthService auth, INavigationService nav, IDriverService drivers)
            : base(auth, nav) { _drivers = drivers; Title = "Drivers"; }

        public override async Task InitializeAsync()
        {
            OnPropertyChanged(nameof(CanAdd));
            OnPropertyChanged(nameof(CanEdit));
            OnPropertyChanged(nameof(CanDelete));
            await LoadAsync();
        }

        public override async Task RefreshOnReturnAsync() => await LoadAsync();

        // Re-load when filter chip changes
        partial void OnSelectedFilterChanged(string value) => LoadCommand.ExecuteAsync(null);

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                CurrentPage = 1;
                var data = await _drivers.GetAllAsync(
                    SearchText.Trim().Length > 0 ? SearchText.Trim() : null, CurrentPage, SelectedFilter);
                Items = new ObservableCollection<DriverItem>(data.Items);
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
                var data = await _drivers.GetAllAsync(
                    SearchText.Trim().Length > 0 ? SearchText.Trim() : null, CurrentPage, SelectedFilter);
                foreach (var item in data.Items) Items.Add(item);
                CanLoadMore = data.PageNumber < data.TotalPages;
            });
        }

        [RelayCommand] private async Task SearchAsync() => await LoadAsync();
        [RelayCommand] private Task AddAsync() => Nav.GoToAsync("CoordDriverForm");
        [RelayCommand]
        private Task EditAsync(DriverItem d) =>
            Nav.GoToAsync("CoordDriverForm", new Dictionary<string, object> { ["UserId"] = d.UserId });
        [RelayCommand]
        private Task DetailAsync(DriverItem d) =>
            Nav.GoToAsync("CoordDriverDetail", new Dictionary<string, object> { ["UserId"] = d.UserId });

        [RelayCommand]
        private void Filter(string filter) => SelectedFilter = filter;
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
    }
}
