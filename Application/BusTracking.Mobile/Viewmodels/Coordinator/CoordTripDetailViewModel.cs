namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordTripDetailViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly ITripService _trips;
        [ObservableProperty] private int _tripId;
        [ObservableProperty] private TripItem? _trip;

        public CoordTripDetailViewModel(IAuthService auth, INavigationService nav, ITripService trips)
            : base(auth, nav) { _trips = trips; Title = "Trip Details"; }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("TripId", out var id)) TripId = (int)id;
        }

        public override async Task InitializeAsync() => await LoadAsync();

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                var all = await _trips.GetAllAsync();
                Trip = all.FirstOrDefault(t => t.TripId == TripId);
            });
        }

        [RelayCommand]
        private async Task StartAsync()
        {
            if (!await ConfirmAsync("Start Trip", "Start this trip?")) return;
            var r = await _trips.StartAsync(TripId);
            if (r.Success) await LoadAsync(); else SetError(r.Message);
        }

        [RelayCommand]
        private async Task CancelTripAsync()
        {
            if (!await ConfirmAsync("Cancel Trip", "Cancel this trip?")) return;
            var r = await _trips.CancelAsync(TripId);
            if (r.Success) await LoadAsync(); else SetError(r.Message);
        }
    }
}
