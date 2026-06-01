namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordRouteDetailViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IRouteService _routes;
        [ObservableProperty] private int _routeId;
        [ObservableProperty] private RouteItem? _route;
        [ObservableProperty] private ObservableCollection<StopItem> _stops = [];

        public bool CanEdit   => Can("route.edit");
        public bool CanDelete => Can("route.delete");

        public CoordRouteDetailViewModel(IAuthService auth, INavigationService nav, IRouteService routes)
            : base(auth, nav) { _routes = routes; Title = "Route Details"; }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("RouteId", out var id)) RouteId = (int)id;
        }

        public override async Task InitializeAsync()
        {
            await RunAsync(async () =>
            {
                var all = await _routes.GetAllAsync();
                Route = all.FirstOrDefault(r => r.RouteId == RouteId);
                Stops = new ObservableCollection<StopItem>(await _routes.GetStopsAsync(RouteId));
                OnPropertyChanged(nameof(CanEdit));
                OnPropertyChanged(nameof(CanDelete));
            });
        }

        [RelayCommand]
        private Task EditAsync()
        {
            if (!CanEdit) return Task.CompletedTask;
            return Nav.GoToAsync("CoordRouteForm", new Dictionary<string, object> { ["RouteId"] = RouteId });
        }

        [RelayCommand]
        private async Task DeleteAsync()
        {
            if (!CanDelete) return;
            if (!await ConfirmAsync("Delete Route", "Delete this route? This cannot be undone.")) return;
            var r = await _routes.DeleteAsync(RouteId);
            if (r.Success) { await ShowToastAsync("Route deleted."); await Nav.GoBackAsync(); }
            else SetError(r.Message);
        }
    }
}
