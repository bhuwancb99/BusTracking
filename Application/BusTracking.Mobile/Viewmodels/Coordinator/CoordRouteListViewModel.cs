namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordRouteListViewModel : BaseViewModel
    {
        private readonly IRouteService _routes;

        [ObservableProperty] private ObservableCollection<RouteItem> _items = [];
        [ObservableProperty] private string _searchText = "";

        public string SearchPlaceholder => "Search routes…";
        public bool CanLoadMore => false;
        public bool CanAdd => Can("route.add");
        public bool CanEdit => Can("route.edit");
        public bool CanDelete => Can("route.delete");

        public CoordRouteListViewModel(IAuthService auth, INavigationService nav, IRouteService routes)
            : base(auth, nav) { _routes = routes; Title = "Routes"; }

        public override async Task InitializeAsync() => await LoadAsync();

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                var data = await _routes.GetAllAsync();
                Items = new ObservableCollection<RouteItem>(data);
                IsEmpty = !Items.Any();
            });
        }

        [RelayCommand] private async Task LoadMoreAsync() { }
        [RelayCommand] private async Task SearchAsync() => await LoadAsync();
        [RelayCommand] private Task AddAsync() => Nav.GoToAsync("CoordRouteForm");
        [RelayCommand]
        private Task EditAsync(RouteItem r) =>
            Nav.GoToAsync("CoordRouteForm", new Dictionary<string, object> { ["RouteId"] = r.RouteId });
        [RelayCommand]
        private Task DetailAsync(RouteItem r) =>
            Nav.GoToAsync("CoordRouteDetail", new Dictionary<string, object> { ["RouteId"] = r.RouteId });
    }
}
