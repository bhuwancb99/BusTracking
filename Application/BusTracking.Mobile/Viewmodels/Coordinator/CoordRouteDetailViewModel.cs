namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordRouteDetailViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IRouteService _routes;
        [ObservableProperty] private int _routeId;
        [ObservableProperty] private RouteItem? _route;
        [ObservableProperty] private ObservableCollection<StopItem> _stops = [];
        [ObservableProperty] private bool _hasStops;

        // Add Stop form fields
        [ObservableProperty] private bool _isAddStopVisible;
        [ObservableProperty] private string _newStopName = "";
        [ObservableProperty] private TimeSpan? _newStopMorningTime;
        [ObservableProperty] private TimeSpan? _newStopEveningTime;
        [ObservableProperty] private string _newStopLatitude = "";
        [ObservableProperty] private string _newStopLongitude = "";

        // Stops inline edit mode
        [ObservableProperty] private bool _isEditingStops;

        public bool CanEdit => Can("route.edit");
        public bool CanDelete => Can("route.delete");
        public bool ShowUpdateOrder => CanEdit && HasStops;

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
                var stops = await _routes.GetStopsAsync(RouteId);
                foreach (var s in stops)
                {
                    s.OriginalOrder = s.StopOrder;
                    s.OrderText = s.StopOrder.ToString();
                    s.IsEditing = IsEditingStops;
                }
                Stops = new ObservableCollection<StopItem>(stops);
                HasStops = stops.Count > 0;
                OnPropertyChanged(nameof(CanEdit));
                OnPropertyChanged(nameof(CanDelete));
                OnPropertyChanged(nameof(ShowUpdateOrder));
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
        private void ToggleEditingStops()
        {
            if (!CanEdit) return;
            if (IsEditingStops)
            {
                IsEditingStops = false;
                foreach (var s in Stops) s.IsEditing = false;
                _ = LoadAsync();
            }
            else
            {
                IsEditingStops = true;
                foreach (var s in Stops) s.IsEditing = true;
            }
        }

        [RelayCommand]
        private async Task SaveStopsAsync()
        {
            if (!CanEdit) return;

            var parsed = new List<(StopItem Stop, int NewOrder)>();
            foreach (var s in Stops)
            {
                if (string.IsNullOrWhiteSpace(s.StopName))
                {
                    SetError("Stop name is required for all stops.");
                    return;
                }
                if (!int.TryParse(s.OrderText, out var newOrder))
                {
                    SetError($"'{s.StopName}' has an invalid order number. Please enter a whole number.");
                    return;
                }
                parsed.Add((s, newOrder));
            }

            var orders = parsed.Select(p => p.NewOrder).ToList();
            if (orders.Distinct().Count() != orders.Count)
            {
                SetError("Order numbers must be unique. Please fix duplicate order numbers before updating.");
                return;
            }

            var req = new UpdateStopsRequest
            {
                RouteId = RouteId,
                Stops = Stops.Select(s => new UpdateStopItemRequest
                {
                    StopId = s.StopId,
                    StopName = s.StopName.Trim(),
                    StopOrder = int.Parse(s.OrderText),
                    MorningTime = s.MorningTime,
                    EveningTime = s.EveningTime,
                    Latitude = s.Latitude,
                    Longitude = s.Longitude
                }).ToList()
            };

            bool success = false;
            string? failMessage = null;

            await RunAsync(async () =>
            {
                var r = await _routes.UpdateStopsAsync(req);
                success = r.Success;
                failMessage = r.Message;
            });

            if (success)
            {
                await ShowToastAsync("Stop records updated successfully.");
                IsEditingStops = false;
                foreach (var s in Stops) s.IsEditing = false;
                await LoadAsync();
            }
            else if (failMessage is not null)
            {
                SetError(failMessage);
            }
        }

        [RelayCommand]
        private async Task AddStopAsync()
        {
            if (!CanEdit) return;
            if (string.IsNullOrWhiteSpace(NewStopName))
            { SetError("Stop name is required."); return; }

            decimal? lat = decimal.TryParse(NewStopLatitude, out var la) ? la : null;
            decimal? lng = decimal.TryParse(NewStopLongitude, out var lo) ? lo : null;

            await RunAsync(async () =>
            {
                var req = new CreateStopRequest
                {
                    RouteId = RouteId,
                    StopName = NewStopName,
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
            NewStopName = "";
            NewStopMorningTime = null; NewStopEveningTime = null;
            NewStopLatitude = ""; NewStopLongitude = "";
        }
    }
}
