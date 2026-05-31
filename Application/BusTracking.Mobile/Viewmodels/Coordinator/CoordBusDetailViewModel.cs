namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordBusDetailViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IBusService _buses;
        [ObservableProperty] private int _busId;
        [ObservableProperty] private BusItem? _bus;

        public bool CanEdit => Can("bus.edit");
        public bool CanDelete => Can("bus.delete");

        public CoordBusDetailViewModel(IAuthService auth, INavigationService nav, IBusService buses)
            : base(auth, nav) { _buses = buses; Title = "Bus Details"; }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("BusId", out var id)) BusId = (int)id;
        }

        public override async Task InitializeAsync()
        {
            await RunAsync(async () => { Bus = await _buses.GetByIdAsync(BusId); });
        }

        [RelayCommand]
        private Task EditAsync()
        {
            if (!CanEdit) return Task.CompletedTask;
            return Nav.GoToAsync("CoordBusForm", new Dictionary<string, object> { ["BusId"] = BusId });
        }

        [RelayCommand]
        private async Task ToggleAsync()
        {
            if (Bus is null) return;
            var r = await _buses.ToggleAsync(BusId);
            if (r.Success) { await ShowToastAsync(r.Message); await InitializeAsync(); }
            else SetError(r.Message);
        }

        [RelayCommand]
        private async Task DeleteAsync()
        {
            if (!CanDelete) return;
            if (!await ConfirmAsync("Delete Bus", $"Delete '{Bus?.BusName}'?")) return;
            var r = await _buses.DeleteAsync(BusId);
            if (r.Success) { await ShowToastAsync("Bus deleted."); await Nav.GoBackAsync(); }
            else SetError(r.Message);
        }
    }
}