namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminRouteListViewModel : BaseViewModel
    {
        private readonly IRouteService _routes;

        [ObservableProperty] private ObservableCollection<RouteItem> _items = [];
        [ObservableProperty] private string _searchText = "";
        [ObservableProperty] private int _currentPage = 1;
        [ObservableProperty] private bool _canLoadMore;

        public string SearchPlaceholder => "Search routes…";
        public bool CanAdd => true;
        public bool CanEdit => true;
        public bool CanDelete => true;

        public AdminRouteListViewModel(IAuthService auth, INavigationService nav, IRouteService routes)
            : base(auth, nav) { _routes = routes; Title = "Routes"; }

        public override async Task InitializeAsync() => await LoadAsync();
        public override async Task RefreshOnReturnAsync() => await LoadAsync();

        // ── Load list — standalone, never called inside another RunAsync ──
        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                CurrentPage = 1;
                var data = await _routes.GetAllAsync(
                    SearchText.Trim().Length > 0 ? SearchText.Trim() : null, CurrentPage);
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
                    SearchText.Trim().Length > 0 ? SearchText.Trim() : null, CurrentPage);
                foreach (var item in data.Items) Items.Add(item);
                CanLoadMore = data.PageNumber < data.TotalPages;
            });
        }

        [RelayCommand] private async Task SearchAsync() => await LoadAsync();
        [RelayCommand] private Task AddAsync() => Nav.GoToAsync("AdminRouteForm");

        // Tap row → Detail
        [RelayCommand]
        private Task DetailAsync(RouteItem r) =>
            Nav.GoToAsync("AdminRouteDetail", new Dictionary<string, object> { ["RouteId"] = r.RouteId });

        [RelayCommand]
        private Task EditAsync(RouteItem r) =>
            Nav.GoToAsync("AdminRouteForm", new Dictionary<string, object> { ["RouteId"] = r.RouteId });

        [RelayCommand]
        private async Task DeleteAsync(RouteItem r)
        {
            if (!await ConfirmAsync("Delete Route", $"Delete '{r.RouteName}'?")) return;
            var result = await _routes.DeleteAsync(r.RouteId);
            if (result.Success) { Items.Remove(r); await ShowToastAsync("Route deleted."); }
            else SetError(result.Message);
        }
    }
}