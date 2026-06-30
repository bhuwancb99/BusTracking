namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminRouteFormViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IRouteService _routes;

        [ObservableProperty] private int? _routeId;
        [ObservableProperty] private bool _isEditMode;
        [ObservableProperty] private string _routeName = "";
        [ObservableProperty] private string _routeCode = "";
        [ObservableProperty] private string _description = "";
        [ObservableProperty] private TimeSpan? _morningTime;
        [ObservableProperty] private TimeSpan? _eveningTime;
        [ObservableProperty] private bool _isActive = true;

        public AdminRouteFormViewModel(IAuthService auth, INavigationService nav, IRouteService routes)
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
                var r = await _routes.GetByIdAsync(RouteId.Value);
                if (r is null) return;
                RouteName = r.RouteName; RouteCode = r.RouteCode;
                Description = r.Description ?? "";
                MorningTime = ParseTime(r.MorningTime);
                EveningTime = ParseTime(r.EveningTime);
                IsActive = r.IsActive;
            });
        }

        private static TimeSpan? ParseTime(string? value) =>
            TimeSpan.TryParse(value, out var t) ? t : null;

        private static string? FormatTime(TimeSpan? value) =>
            value.HasValue ? value.Value.ToString(@"hh\:mm") : null;

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(RouteName) || string.IsNullOrWhiteSpace(RouteCode))
            { SetError("Route name and code are required."); return; }

            await RunAsync(async () =>
            {
                var req = new UpdateRouteRequest
                {
                    RouteName = RouteName,
                    RouteCode = RouteCode,
                    MorningTime = FormatTime(MorningTime),
                    EveningTime = FormatTime(EveningTime),
                    Description = Description.Length > 0 ? Description : null,
                    IsActive = IsActive
                };
                var r = IsEditMode
                    ? await _routes.UpdateAsync(RouteId!.Value, req)
                    : await _routes.CreateAsync(new CreateRouteRequest
                    {
                        RouteName = RouteName,
                        RouteCode = RouteCode,
                        MorningTime = req.MorningTime,
                        EveningTime = req.EveningTime,
                        Description = req.Description,
                        IsActive = IsActive
                    });

                if (r.Success) { await ShowToastAsync(IsEditMode ? "Route updated." : "Route created."); await Nav.GoBackAsync(); }
                else SetError(r.Message);
            });
        }

        [RelayCommand] private Task CancelAsync() => Nav.GoBackAsync();
    }
}
