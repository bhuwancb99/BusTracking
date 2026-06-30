namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordRouteDetailViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IRouteService _routes;
        [ObservableProperty] private int _routeId;
        [ObservableProperty] private RouteItem? _route;
        [ObservableProperty] private ObservableCollection<StopItem> _stops = [];

        // Add Stop form fields
        [ObservableProperty] private bool _isAddStopVisible;
        [ObservableProperty] private string _newStopName = "";
        [ObservableProperty] private string _newStopOrder = "";
        [ObservableProperty] private TimeSpan? _newStopMorningTime;
        [ObservableProperty] private TimeSpan? _newStopEveningTime;
        [ObservableProperty] private string _newStopLatitude = "";
        [ObservableProperty] private string _newStopLongitude = "";

        public bool CanEdit   => Can("route.edit");
        public bool CanDelete => Can("route.delete");

        public CoordRouteDetailViewModel(IAuthService auth, INavigationService nav, IRouteService routes)
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
                Route = await _routes.GetByIdAsync(RouteId);
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

        [RelayCommand]
        private void ToggleAddStop()
        {
            if (!CanEdit) return;
            IsAddStopVisible = !IsAddStopVisible;
        }

        [RelayCommand]
        private async Task AddStopAsync()
        {
            if (!CanEdit) return;
            if (string.IsNullOrWhiteSpace(NewStopName) || string.IsNullOrWhiteSpace(NewStopOrder))
            { SetError("Stop name and order are required."); return; }

            if (!int.TryParse(NewStopOrder, out var order))
            { SetError("Stop order must be a number."); return; }

            decimal? lat = decimal.TryParse(NewStopLatitude, out var la) ? la : null;
            decimal? lng = decimal.TryParse(NewStopLongitude, out var lo) ? lo : null;

            await RunAsync(async () =>
            {
                var req = new CreateStopRequest
                {
                    RouteId = RouteId,
                    StopName = NewStopName,
                    StopOrder = order,
                    MorningTime = NewStopMorningTime.HasValue ? NewStopMorningTime.Value.ToString(@"hh\:mm") : null,
                    EveningTime = NewStopEveningTime.HasValue ? NewStopEveningTime.Value.ToString(@"hh\:mm") : null,
                    Latitude = lat,
                    Longitude = lng
                };
                var r = await _routes.AddStopAsync(req);
                if (r.Success)
                {
                    await ShowToastAsync("Stop added.");
                    ResetAddStopForm();
                    IsAddStopVisible = false;
                    await LoadAsync(); // refresh stop list + stop count
                }
                else SetError(r.Message);
            });
        }

        [RelayCommand]
        private async Task DeleteStopAsync(StopItem stop)
        {
            if (!CanEdit) return;
            if (!await ConfirmAsync("Remove Stop", $"Remove '{stop.StopName}'?")) return;
            var r = await _routes.DeleteStopAsync(stop.StopId, RouteId);
            if (r.Success) { await ShowToastAsync("Stop removed."); await LoadAsync(); }
            else SetError(r.Message);
        }

        private void ResetAddStopForm()
        {
            NewStopName = ""; NewStopOrder = "";
            NewStopMorningTime = null; NewStopEveningTime = null;
            NewStopLatitude = ""; NewStopLongitude = "";
        }
    }
}
