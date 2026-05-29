namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminRouteListViewModel : BaseViewModel
    {
        private readonly IRouteService _routes;

        [ObservableProperty] private ObservableCollection<RouteItem> _items = [];
        [ObservableProperty] private string _searchText = "";

        // SuperAdmin always has full access
        public bool CanAdd => true;
        public bool CanEdit => true;
        public bool CanDelete => true;
        public bool CanView => true;

        public AdminRouteListViewModel(IAuthService auth, INavigationService nav, IRouteService routes)
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

        [RelayCommand]
        private Task AddAsync() => Nav.GoToAsync("AdminRouteForm");

        [RelayCommand]
        private Task EditAsync(RouteItem r) =>
            Nav.GoToAsync("AdminRouteForm", new Dictionary<string, object> { ["RouteId"] = r.RouteId });

        [RelayCommand]
        private Task ViewAsync(RouteItem r) =>
            Nav.GoToAsync("AdminRouteDetail", new Dictionary<string, object> { ["RouteId"] = r.RouteId });

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