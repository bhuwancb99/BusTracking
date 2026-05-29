namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminBusDetailViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IBusService _buses;

        [ObservableProperty] private int _busId;
        [ObservableProperty] private BusItem? _bus;

        public AdminBusDetailViewModel(IAuthService auth, INavigationService nav, IBusService buses)
            : base(auth, nav) { _buses = buses; Title = "Bus Details"; }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("BusId", out var id)) BusId = (int)id;
        }

        public override async Task InitializeAsync() => await LoadAsync();

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                Bus = await _buses.GetByIdAsync(BusId);
            });
        }

        [RelayCommand]
        private Task EditAsync() =>
            Nav.GoToAsync("AdminBusForm", new Dictionary<string, object> { ["BusId"] = BusId });

        [RelayCommand]
        private async Task ToggleAsync()
        {
            if (Bus is null) return;
            var r = await _buses.ToggleAsync(BusId);
            if (r.Success) { await ShowToastAsync(r.Message); await LoadAsync(); }
            else SetError(r.Message);
        }

        [RelayCommand]
        private async Task DeleteAsync()
        {
            if (!await ConfirmAsync("Delete Bus", $"Delete '{Bus?.BusName}'?")) return;
            var r = await _buses.DeleteAsync(BusId);
            if (r.Success) { await ShowToastAsync("Bus deleted."); await Nav.GoBackAsync(); }
            else SetError(r.Message);
        }
    }
}
