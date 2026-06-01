namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminRouteListViewModel : BaseViewModel
    {
        private readonly IRouteService _routes;

        [ObservableProperty] private ObservableCollection<RouteItem> _items = [];
        [ObservableProperty] private string _searchText = "";

        public bool CanAdd => true;
        public bool CanEdit => true;
        public bool CanDelete => true;

        public AdminRouteListViewModel(IAuthService auth, INavigationService nav, IRouteService routes)
            : base(auth, nav) { _routes = routes; Title = "Routes"; }

        public override async Task InitializeAsync() => await LoadAsync();
        public override async Task RefreshOnReturnAsync() => await LoadAsync();

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                var data = await _routes.GetAllAsync();
                // Client-side search since API doesn't support it for routes
                if (!string.IsNullOrWhiteSpace(SearchText))
                    data = data.Where(r =>
                        r.RouteName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        r.RouteCode.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).ToList();
                Items = new ObservableCollection<RouteItem>(data);
                IsEmpty = !Items.Any();
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