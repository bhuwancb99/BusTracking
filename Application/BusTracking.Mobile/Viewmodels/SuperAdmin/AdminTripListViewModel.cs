namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminTripListViewModel : BaseViewModel
    {
        private readonly ITripService _trips;

        [ObservableProperty] private ObservableCollection<TripItem> _items = [];
        [ObservableProperty] private string _selectedStatus = "All";
        [ObservableProperty] private DateTime _selectedDate = DateTime.Today;
        [ObservableProperty] private int _currentPage = 1;
        [ObservableProperty] private bool _canLoadMore;

        public bool CanAdd => Can("trip.manage");
        public List<string> StatusOptions => ["All", "Scheduled", "InProgress", "Completed", "Cancelled"];

        public AdminTripListViewModel(IAuthService auth, INavigationService nav, ITripService trips)
            : base(auth, nav) { _trips = trips; Title = "Trips"; }

        public override async Task InitializeAsync() => await LoadAsync();
        public override async Task RefreshOnReturnAsync() => await LoadAsync();

        // Status chip changed → reload current date with new filter
        partial void OnSelectedStatusChanged(string value) => LoadCommand.ExecuteAsync(null);

        // Date changed → reset status to "All" and reload that date's trips
        partial void OnSelectedDateChanged(DateTime value)
        {
            if (SelectedStatus != "All") SelectedStatus = "All";
            else LoadCommand.ExecuteAsync(null);
        }

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                CurrentPage = 1;
                var data = await _trips.GetAllAsync(
                    SelectedStatus != "All" ? SelectedStatus : null,
                    SelectedDate.ToString("yyyy-MM-dd"),
                    CurrentPage);
                Items = new ObservableCollection<TripItem>(data.Items);
                IsEmpty = !Items.Any();
                CanLoadMore = data.PageNumber < data.TotalPages;
            });
        }

        [RelayCommand]
        private async Task LoadMoreAsync()
        {
            if (!CanLoadMore || IsBusy) return;
            await RunAsync(async () =>
            {
                CurrentPage++;
                var data = await _trips.GetAllAsync(
                    SelectedStatus != "All" ? SelectedStatus : null,
                    SelectedDate.ToString("yyyy-MM-dd"),
                    CurrentPage);
                foreach (var item in data.Items) Items.Add(item);
                CanLoadMore = data.PageNumber < data.TotalPages;
            });
        }

        [RelayCommand] private Task AddAsync() => Nav.GoToAsync("AdminTripForm");
        [RelayCommand]
        private Task DetailAsync(TripItem t) =>
            Nav.GoToAsync("AdminTripDetail", new Dictionary<string, object> { ["TripId"] = t.TripId });

        // ── NEW: open live map for an InProgress trip ─────────────────────
        [RelayCommand]
        private async Task TrackLiveAsync(TripItem t)
        {
            await RunAsync(async () =>
            {
                await Nav.GoToAsync("LiveTracking", new Dictionary<string, object> { ["TripId"] = t.TripId });
            });
        }

        [RelayCommand]
        private async Task StartAsync(TripItem t)
        {
            if (!await ConfirmAsync("Start Trip", $"Start trip #{t.TripId}?")) return;
            var r = await _trips.StartAsync(t.TripId);
            if (r.Success) await LoadAsync(); else SetError(r.Message);
        }

        [RelayCommand]
        private async Task CancelTripAsync(TripItem t)
        {
            if (!await ConfirmAsync("Cancel Trip", $"Cancel trip #{t.TripId}?")) return;
            var r = await _trips.CancelAsync(t.TripId);
            if (r.Success) await LoadAsync(); else SetError(r.Message);
        }

        [RelayCommand]
        private void Filter(string status) => SelectedStatus = status;
        [RelayCommand]
        private async Task RefreshAsync()
        {
            IsRefreshing = true;
            try
            {
                await LoadAsync();
            }
            finally
            {
                IsRefreshing = false;
            }
        }
    }
}
