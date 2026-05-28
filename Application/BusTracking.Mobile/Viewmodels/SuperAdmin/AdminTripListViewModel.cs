namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminTripListViewModel : BaseViewModel
    {
        private readonly ITripService _trips;

        [ObservableProperty] private ObservableCollection<TripItem> _items = [];
        [ObservableProperty] private string _selectedStatus = "";
        [ObservableProperty] private string _selectedDate = "";

        public List<string> StatusOptions => ["", "Scheduled", "InProgress", "Completed", "Cancelled"];

        public AdminTripListViewModel(IAuthService auth, INavigationService nav, ITripService trips)
            : base(auth, nav) { _trips = trips; Title = "Trips"; }

        public override async Task InitializeAsync() => await LoadAsync();

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                var data = await _trips.GetAllAsync(
                    SelectedStatus.Length > 0 ? SelectedStatus : null,
                    SelectedDate.Length > 0 ? SelectedDate : null);
                Items = new ObservableCollection<TripItem>(data);
                IsEmpty = !Items.Any();
            });
        }

        [RelayCommand] private Task AddAsync() => Nav.GoToAsync("AdminTripForm");
        [RelayCommand]
        private Task ViewAsync(TripItem t) =>
            Nav.GoToAsync("AdminTripDetail", new Dictionary<string, object> { ["TripId"] = t.TripId });

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
    }
}
