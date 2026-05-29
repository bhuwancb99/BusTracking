namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminRouteDetailViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IRouteService _routes;

        [ObservableProperty] private int _routeId;
        [ObservableProperty] private RouteItem? _route;
        [ObservableProperty] private ObservableCollection<StopItem> _stops = [];

        public AdminRouteDetailViewModel(IAuthService auth, INavigationService nav, IRouteService routes)
            : base(auth, nav) { _routes = routes; Title = "Route Details"; }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("RouteId", out var id)) RouteId = (int)id;
        }

        public override async Task InitializeAsync() => await LoadAsync();

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                var all = await _routes.GetAllAsync();
                Route = all.FirstOrDefault(r => r.RouteId == RouteId);
                var stops = await _routes.GetStopsAsync(RouteId);
                Stops = new ObservableCollection<StopItem>(stops);
            });
        }

        [RelayCommand]
        private Task EditAsync() =>
            Nav.GoToAsync("AdminRouteForm", new Dictionary<string, object> { ["RouteId"] = RouteId });

        [RelayCommand]
        private async Task DeleteAsync()
        {
            if (!await ConfirmAsync("Delete Route", $"Delete '{Route?.RouteName}'?")) return;
            var r = await _routes.DeleteAsync(RouteId);
            if (r.Success) { await ShowToastAsync("Route deleted."); await Nav.GoBackAsync(); }
            else SetError(r.Message);
        }
    }
}
