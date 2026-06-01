namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordTripListViewModel : BaseViewModel
    {
        private readonly ITripService _trips;

        [ObservableProperty] private ObservableCollection<TripItem> _items = [];
        [ObservableProperty] private string _selectedStatus = "";
        [ObservableProperty] private string _searchText = "";
        [ObservableProperty] private bool _canLoadMore;

        public string SearchPlaceholder => "Search trips…";
        public bool CanAdd => Can("trip.manage");
        public List<string> StatusOptions => ["", "Scheduled", "InProgress", "Completed", "Cancelled"];

        public CoordTripListViewModel(IAuthService auth, INavigationService nav, ITripService trips)
            : base(auth, nav) { _trips = trips; Title = "Trips"; }

        public override async Task InitializeAsync() => await LoadAsync();

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                var data = await _trips.GetAllAsync(SelectedStatus.Length > 0 ? SelectedStatus : null);
                Items = new ObservableCollection<TripItem>(data);
                IsEmpty = !Items.Any();
                CanLoadMore = false;
            });
        }

        [RelayCommand] private async Task LoadMoreAsync() { }
        [RelayCommand] private async Task SearchAsync() => await LoadAsync();
        [RelayCommand] private Task AddAsync() => Nav.GoToAsync("CoordTripForm");
        [RelayCommand]
        private Task DetailAsync(TripItem t) =>
            Nav.GoToAsync("CoordTripDetail", new Dictionary<string, object> { ["TripId"] = t.TripId });

        [RelayCommand]
        private async Task StartAsync(TripItem t)
        {
            if (!await ConfirmAsync("Start Trip", $"Start trip #{t.TripId}?")) return;
            var r = await _trips.StartAsync(t.TripId);
            if (r.Success) await LoadAsync(); else SetError(r.Message);
        }

        [RelayCommand]
        private async Task EndAsync(TripItem t)
        {
            if (!await ConfirmAsync("End Trip", $"End trip #{t.TripId}?")) return;
            var r = await _trips.EndAsync(t.TripId);
            if (r.Success) await LoadAsync(); else SetError(r.Message);
        }

        [RelayCommand]
        private async Task CancelTripAsync(TripItem t)
        {
            if (!await ConfirmAsync("Cancel Trip", "Are you sure?")) return;
            var r = await _trips.CancelAsync(t.TripId);
            if (r.Success) await LoadAsync(); else SetError(r.Message);
        }
    }
}
