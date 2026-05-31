namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordRouteListViewModel : BaseViewModel
    {
        private readonly IRouteService _routes;

        [ObservableProperty] private ObservableCollection<RouteItem> _items = [];

        public bool CanView => Can("route.view");
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
                var data = await _routes.GetAllAsync();
                Items = new ObservableCollection<RouteItem>(data);
                IsEmpty = !Items.Any();
            });
        }

        [RelayCommand]
        private Task AddAsync()
        {
            if (!CanAdd) return Task.CompletedTask;
            return Nav.GoToAsync("CoordRouteForm");
        }

        [RelayCommand]
        private Task EditAsync(RouteItem r)
        {
            if (!CanEdit) return Task.CompletedTask;
            return Nav.GoToAsync("CoordRouteForm", new Dictionary<string, object> { ["RouteId"] = r.RouteId });
        }

        [RelayCommand]
        private Task ViewAsync(RouteItem r)
        {
            if (!CanView) return Task.CompletedTask;
            return Nav.GoToAsync("CoordRouteDetail", new Dictionary<string, object> { ["RouteId"] = r.RouteId });
        }

        [RelayCommand]
        private async Task DeleteAsync(RouteItem r)
        {
            if (!CanDelete) return;
            if (!await ConfirmAsync("Delete Route", $"Delete '{r.RouteName}'?")) return;
            var result = await _routes.DeleteAsync(r.RouteId);
            if (result.Success) { Items.Remove(r); await ShowToastAsync("Route deleted."); }
            else SetError(result.Message);
        }
    }
}