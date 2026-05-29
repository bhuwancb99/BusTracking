namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordRouteFormViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IRouteService _routes;

        [ObservableProperty] private int? _routeId;
        [ObservableProperty] private bool _isEditMode;
        [ObservableProperty] private string _routeName = "";
        [ObservableProperty] private string _routeCode = "";
        [ObservableProperty] private string _morningTime = "";
        [ObservableProperty] private string _eveningTime = "";
        [ObservableProperty] private bool _isActive = true;

        public CoordRouteFormViewModel(IAuthService auth, INavigationService nav, IRouteService routes)
            : base(auth, nav) { _routes = routes; }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("RouteId", out var id)) { RouteId = (int)id; IsEditMode = true; Title = "Edit Route"; }
            else Title = "Add Route";
        }

        public override async Task InitializeAsync()
        {
            if (!IsEditMode || !RouteId.HasValue) return;
            await RunAsync(async () =>
            {
                var all = await _routes.GetAllAsync();
                var r = all.FirstOrDefault(x => x.RouteId == RouteId.Value);
                if (r is null) return;
                RouteName = r.RouteName; RouteCode = r.RouteCode;
                MorningTime = r.MorningTime ?? ""; EveningTime = r.EveningTime ?? "";
                IsActive = r.IsActive;
            });
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(RouteName) || string.IsNullOrWhiteSpace(RouteCode))
            { SetError("Route name and code are required."); return; }

            await RunAsync(async () =>
            {
                var r = IsEditMode
                    ? await _routes.UpdateAsync(RouteId!.Value, new UpdateRouteRequest
                    {
                        RouteName = RouteName,
                        RouteCode = RouteCode,
                        MorningTime = MorningTime.Length > 0 ? MorningTime : null,
                        EveningTime = EveningTime.Length > 0 ? EveningTime : null,
                        IsActive = IsActive
                    })
                    : await _routes.CreateAsync(new CreateRouteRequest
                    {
                        RouteName = RouteName,
                        RouteCode = RouteCode,
                        MorningTime = MorningTime.Length > 0 ? MorningTime : null,
                        EveningTime = EveningTime.Length > 0 ? EveningTime : null,
                        IsActive = IsActive
                    });

                if (r.Success) { await ShowToastAsync(IsEditMode ? "Route updated." : "Route created."); await Nav.GoBackAsync(); }
                else SetError(r.Message);
            });
        }

        [RelayCommand] private Task CancelAsync() => Nav.GoBackAsync();
    }
}
