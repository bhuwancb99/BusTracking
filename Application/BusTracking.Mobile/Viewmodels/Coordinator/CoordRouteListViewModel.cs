namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordRouteListViewModel : BaseViewModel
    {
        private readonly IRouteService _routes;

        [ObservableProperty] private ObservableCollection<RouteItem> _items = [];
        [ObservableProperty] private string _searchText = "";
        [ObservableProperty] private string _selectedStatus = "Active";
        [ObservableProperty] private int _currentPage = 1;
        [ObservableProperty] private bool _canLoadMore;

        public string SearchPlaceholder => "Search routes…";
        public List<string> StatusOptions => ["Active", "Inactive", "Both"];
        public bool CanAdd => Can("route.add");
        public bool CanEdit => Can("route.edit");
        public bool CanDelete => Can("route.delete");

        public CoordRouteListViewModel(IAuthService auth, INavigationService nav, IRouteService routes)
            : base(auth, nav) { _routes = routes; Title = "Routes"; }

        public override async Task InitializeAsync() => await LoadAsync();
        public override async Task RefreshOnReturnAsync() => await LoadAsync();

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                CurrentPage = 1;
                var data = await _routes.GetAllAsync(
                    SearchText.Trim().Length > 0 ? SearchText.Trim() : null, CurrentPage, SelectedStatus);
                Items = new ObservableCollection<RouteItem>(data.Items);
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
                var data = await _routes.GetAllAsync(
                    SearchText.Trim().Length > 0 ? SearchText.Trim() : null, CurrentPage, SelectedStatus);
                foreach (var item in data.Items) Items.Add(item);
                CanLoadMore = data.PageNumber < data.TotalPages;
            });
        }

        [RelayCommand] private async Task SearchAsync() => await LoadAsync();

        // Filter re-loads when status picker changes
        partial void OnSelectedStatusChanged(string value) => LoadCommand.ExecuteAsync(null);

        [RelayCommand] private Task AddAsync() => Nav.GoToAsync("CoordRouteForm");
        [RelayCommand]
        private Task EditAsync(RouteItem r) =>
            Nav.GoToAsync("CoordRouteForm", new Dictionary<string, object> { ["RouteId"] = r.RouteId });
        [RelayCommand]
        private Task DetailAsync(RouteItem r) =>
            Nav.GoToAsync("CoordRouteDetail", new Dictionary<string, object> { ["RouteId"] = r.RouteId });
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