namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordTripListViewModel : BaseViewModel
    {
        private readonly ITripService _trips;

        [ObservableProperty] private ObservableCollection<TripItem> _items = [];
        [ObservableProperty] private string _selectedStatus = "";

        // trip.view  = can see the list and details
        // trip.manage = can create / start / end / cancel
        public bool CanView => Can("trip.view") || Can("trip.manage");
        public bool CanManage => Can("trip.manage");

        public List<string> StatusOptions => ["", "Scheduled", "InProgress", "Completed", "Cancelled"];

        public CoordTripListViewModel(IAuthService auth, INavigationService nav, ITripService trips)
            : base(auth, nav) { _trips = trips; Title = "Trips"; }

        public override async Task InitializeAsync() => await LoadAsync();
        public override async Task RefreshOnReturnAsync() => await LoadAsync();

        partial void OnSelectedStatusChanged(string value) => LoadCommand.ExecuteAsync(null);

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                var data = await _trips.GetAllAsync(SelectedStatus.Length > 0 ? SelectedStatus : null);
                Items = new ObservableCollection<TripItem>(data);
                IsEmpty = !Items.Any();
            });
        }

        [RelayCommand]
        private Task AddAsync()
        {
            if (!CanManage) return Task.CompletedTask;
            return Nav.GoToAsync("CoordTripForm");
        }

        [RelayCommand]
        private Task ViewAsync(TripItem t)
        {
            if (!CanView) return Task.CompletedTask;
            return Nav.GoToAsync("CoordTripDetail", new Dictionary<string, object> { ["TripId"] = t.TripId });
        }

        [RelayCommand]
        private async Task StartAsync(TripItem t)
        {
            if (!CanManage) return;
            if (!await ConfirmAsync("Start Trip", $"Start trip for {t.BusNumber}?")) return;
            var r = await _trips.StartAsync(t.TripId);
            if (r.Success) await LoadAsync(); else SetError(r.Message);
        }

        [RelayCommand]
        private async Task EndAsync(TripItem t)
        {
            if (!CanManage) return;
            if (!await ConfirmAsync("End Trip", $"End trip for {t.BusNumber}?")) return;
            var r = await _trips.EndAsync(t.TripId);
            if (r.Success) await LoadAsync(); else SetError(r.Message);
        }

        [RelayCommand]
        private async Task CancelTripAsync(TripItem t)
        {
            if (!CanManage) return;
            if (!await ConfirmAsync("Cancel Trip", "Are you sure?")) return;
            var r = await _trips.CancelAsync(t.TripId);
            if (r.Success) await LoadAsync(); else SetError(r.Message);
        }
    }
}